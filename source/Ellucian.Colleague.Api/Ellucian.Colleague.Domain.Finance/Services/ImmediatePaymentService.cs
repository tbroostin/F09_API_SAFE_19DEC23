// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Ellucian.Colleague.Domain.Finance.Repositories;
using slf4net;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    public class ImmediatePaymentService : IImmediatePaymentService
    {
        private ILogger _logger;

        /// <summary>
        /// Constructor for the immediate payment service
        /// </summary>
        /// <param name="paymentPlanProcessor">Payment Plan Processor service</param>
        /// <param name="arRepository">Interface to AccountsReceivableRepository</param>
        /// <param name="logger">The Logger</param>
        public ImmediatePaymentService(ILogger logger)
        {
            _logger = logger;
        }

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
        public ImmediatePaymentOptions GetPaymentOptions(RegistrationPaymentControl paymentControl, IEnumerable<Invoice> invoices, IEnumerable<ReceivablePayment> payments,
            List<AccountTerm> accountTerms, Dictionary<string, string> distributionMap, PaymentRequirement payReq, IEnumerable<ChargeCode> chargeCodes,
            IEnumerable<string> receivableTypes = null)
        {
            if (paymentControl == null)
            {
                throw new ArgumentNullException("paymentControl", "A registration payment control must be provided");
            }
            if (invoices == null || invoices.Count() == 0)
            {
                throw new ArgumentNullException("invoices", "At least one invoice must be provided.");
            }
            if (distributionMap == null || distributionMap.Count == 0)
            {
                throw new ArgumentNullException("distributionMap", "The distribution map must have at least one entry.");
            }
            if (payments == null)
            {
                payments = new List<ReceivablePayment>();
            }
            if (payReq == null)
            {
                // No requirements were specified, so use a deferral of 100% with no payment plan options
                var deferralOption = new PaymentDeferralOption(DateTime.MaxValue, null, 100);
                var deferralOptions = new List<PaymentDeferralOption>() { deferralOption };
                payReq = new PaymentRequirement("temp", paymentControl.TermId, null, 0, deferralOptions, null);
            }

            // Set the default values for the payment options
            bool chargesOnPaymentPlan = false;
            decimal regBalance = 0;
            decimal minPayment = 0;
            decimal defPercent = 100;

            // Build the list of receivable types if it isn't provided.  Filter the invoices and payments
            // to just the receivable types in the list.
            if (receivableTypes == null)
            {
                // No AR type restrictions - get all the AR types on the invoices
                receivableTypes = invoices.Select(x => x.ReceivableTypeCode).Distinct();
            }
            else
            {
                // Filter the invoices using the specified AR types
                invoices = invoices.Where(x => receivableTypes.Contains(x.ReceivableTypeCode));
            }
            if (paymentControl.Payments != null && paymentControl.Payments.Count() > 0)
            {
                payments = payments.Where(x => receivableTypes.Contains(x.ReceivableType)).ToList();
            }

            // If no invoices remain after filtering, return the default options
            if (invoices.Count() == 0)
            {
                return new ImmediatePaymentOptions(chargesOnPaymentPlan, regBalance, minPayment, defPercent);
            }

            // Calculate the total invoices and total payments
            decimal totalInvoices = invoices.Sum(x => x.Amount);
            decimal totalPayments = payments.Sum(x => x.Amount);

            // Determine whether or not any of the IPC charges are associated with a payment plan
            chargesOnPaymentPlan = invoices.Any(x => x.Charges.Any(y => y.PaymentPlanIds.Count() > 0));

            // If any charges are associated with a payment plan, return the default options
            if (chargesOnPaymentPlan)
            {
                return new ImmediatePaymentOptions(chargesOnPaymentPlan, regBalance, minPayment, defPercent);
            }

            // Calculate the registration balance by subtracting the total payment amount from the total invoice amount
            regBalance = totalInvoices - totalPayments;

            // If the student has a zero or credit registration balance, set the balance to zero, and return the default options
            if (regBalance <= 0)
            {
                regBalance = 0;
                return new ImmediatePaymentOptions(chargesOnPaymentPlan, regBalance, minPayment, defPercent);
            }

            // Otherwise, get the term balance for comparison
            decimal termBalance = GetTermBalance(accountTerms, receivableTypes, paymentControl.TermId);

            // If the term balance is zero or a credit, set the balance to zero, and return the default options
            if (termBalance <= 0)
            {
                regBalance = 0;
                return new ImmediatePaymentOptions(chargesOnPaymentPlan, regBalance, minPayment, defPercent);
            }

            // The term balance is a debit; if it's less than the registration balance, then set the registration balance to the term balance
            regBalance = Math.Min(regBalance, termBalance);

            // Determine the deferral percentage for the student and calculate the minimum payment
            defPercent = GetDeferralPercentage(payReq);
            minPayment = CalculateMinimumPayment(totalInvoices, totalPayments, termBalance, defPercent);

            return new ImmediatePaymentOptions(chargesOnPaymentPlan, regBalance, minPayment, defPercent);
        }

        /// <summary>
        /// Add payment plan information to the payment options
        /// </summary>
        /// <param name="paymentOptions">The payment options to update</param>
        /// <param name="payPlanTemplate">The payment plan template to use for a new plan</param>
        /// <param name="firstPaymentDate">Date of the first scheduled payment on a new plan</param>
        /// <param name="payPlanAmount">Amount of the new payment plan</param>
        /// <param name="payPlanReceivableTypeCode">Receivable type of the new plan</param>
        public void AddPaymentPlanOption(ImmediatePaymentOptions paymentOptions, PaymentPlanTemplate payPlanTemplate, DateTime? firstPaymentDate, decimal payPlanAmount,
            string payPlanReceivableTypeCode)
        {
            if (payPlanTemplate == null)
            {
                throw new ArgumentNullException("payPlanTemplate", "A payment plan template must be specified to add plan information to payment options.");
            }
            
            if (!firstPaymentDate.HasValue)
            {
                throw new ArgumentNullException("firstPaymentDate", "A first payment date must be specified to add plan information to payment options.");
            }
        
            // Payment plan creation is permitted - set payment plan properties accordingly and return student payment options
            DateTime payPlanFirstDueDate = firstPaymentDate.GetValueOrDefault();
            decimal downPaymentAmount = payPlanTemplate.CalculateDownPaymentAmount(payPlanAmount);
            DateTime downPaymentDate = DateTime.Today;

            // A down payment exists - set the down payment due date using the Days Until Down Payment Is Due template parameter
            if (downPaymentAmount > 0)
            {
                downPaymentDate = downPaymentDate.AddDays(payPlanTemplate.DaysUntilDownPaymentIsDue);
            }

            paymentOptions.AddPaymentPlanInformation(payPlanTemplate.Id, payPlanFirstDueDate, payPlanAmount, payPlanReceivableTypeCode, downPaymentAmount, downPaymentDate);
        }

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
        public IEnumerable<Payment> GetPaymentSummary(RegistrationPaymentControl paymentControl, string payMethod, decimal amount, IEnumerable<Invoice> invoices, 
            IEnumerable<ReceivablePayment> regPayments, List<AccountTerm> accountTerms, Dictionary<string, string> distributionMap, PaymentRequirement payReq,
            Dictionary<string, PaymentConfirmation> confMap, IEnumerable<ReceivableType> receivableTypes)
        {
            if (paymentControl == null)
            {
                throw new ArgumentNullException("paymentControl", "A registration payment control must be provided");
            }
            if (string.IsNullOrEmpty(payMethod))
            {
                throw new ArgumentNullException("payMethod", "A payment method must be provided");
            }
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Amount must be a positive number.");
            }

            // Get the payment options using the passed-in payment control
            var paymentOptions = GetPaymentOptions(paymentControl, invoices, regPayments, accountTerms, distributionMap, payReq, null, null);
            var paymentItems = BuildPaymentItems(paymentControl, amount, invoices, regPayments, accountTerms, distributionMap, payReq, receivableTypes);
            var payments = BuildPayments(paymentItems, paymentControl.StudentId, payMethod, distributionMap);
            for (int i = 0; i < payments.Count; i++)
            {
                var payment = payments[i];
                var ecInfo = confMap[payment.Distribution];
                payment.ConvenienceFee = ecInfo.ConvenienceFeeCode;
                payment.ConvenienceFeeAmount = ecInfo.ConvenienceFeeAmount;
                payment.ConvenienceFeeGeneralLedgerNumber = ecInfo.ConvenienceFeeGeneralLedgerNumber;
                payment.ProviderAccount = ecInfo.ProviderAccount;
                payment.AmountToPay = payment.PaymentItems.Sum(x => x.PaymentAmount) + payment.ConvenienceFeeAmount.GetValueOrDefault();
                payments[i] = payment;
            }

            return payments.OrderBy(x => x.Distribution);
        }

        /// <summary>
        /// Gets the payment plan option for the payment requirement for the student, if applicable, for the current date
        /// </summary>
        /// <param name="applicablePaymentRequirement">The payment requirement that is applicable for the student</param>
        public PaymentPlanOption GetPaymentPlanOption(PaymentRequirement applicablePaymentRequirement)
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

        /// <summary>
        /// Determines if payment plan template can be used to create payment plans through the Immediate Payment Control workflow
        /// </summary>
        /// <param name="payPlanTemplate">Payment Plan Template</param>
        /// <returns>Whether or not the payment plan template can be used to create payment plans through the Immediate Payment Control workflow</returns>
        public bool IsValidTemplate(PaymentPlanTemplate payPlanTemplate)
        {
            string loggerMessage = null;

            if (payPlanTemplate == null)
            {
                throw new ArgumentNullException("payPlanTemplate", "A payment plan template must be provided.");
            }

            // Template must be Active
            if (!payPlanTemplate.IsActive)
            {
                loggerMessage = "Payment Plan Template {0} is not active; Active = N.";
                _logger.Info(string.Format(loggerMessage, payPlanTemplate.Id));
                return false;
            }

            // Template must be set to automatically calculate plan amounts
            if (!payPlanTemplate.CalculatePlanAmountAutomatically)
            {
                loggerMessage = "Payment Plan Template {0} does not automatically calculate plan amounts; Auto Calculate Plan Amount = N.";
                _logger.Info(string.Format(loggerMessage, payPlanTemplate.Id));
                return false;
            }

            // Template must be set to automatically modify plans when charges are adjusted
            if (!payPlanTemplate.ModifyPlanAutomatically)
            {
                loggerMessage = "Payment Plan Template {0} does not automatically modify plans when plan charges are adjusted; Modify Plan Automatically = N.";
                _logger.Info(string.Format(loggerMessage, payPlanTemplate.Id));
                return false;
            }

            // Template must have a terms and conditions document defined
            if (string.IsNullOrEmpty(payPlanTemplate.TermsAndConditionsDocumentId))
            {
                loggerMessage = "Payment Plan Template {0} does not have a Terms and Conditions Document specified.";
                _logger.Info(string.Format(loggerMessage, payPlanTemplate.Id));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Build the list of payments for a student's payment items
        /// </summary>
        /// <param name="paymentItems">List of payment items</param>
        /// <param name="studentId">Student ID</param>
        /// <param name="payMethod">Payment method code</param>
        /// <param name="distrMap">Mapping of receivable types to distributions</param>
        /// <returns>List of payments to make</returns>
        private List<Payment> BuildPayments(List<PaymentItem> paymentItems, string studentId, string payMethod, Dictionary<string, string> distrMap)
        {
            var payments = new List<Payment>();
            foreach (var item in paymentItems)
            {
                var distribution = distrMap[item.AccountType];
                // Add this item to an existing payment with this distribution, or create a new payment
                int pos = payments.FindIndex(x => x.Distribution == distribution);
                if (pos >= 0)
                {
                    // Add item to existing payment
                    payments[pos].AmountToPay += item.PaymentAmount;
                    payments[pos].PaymentItems.Add(item);
                }
                else
                {
                    // Add a new payment
                    var payment = new Payment()
                    {
                        AmountToPay = item.PaymentAmount,
                        Distribution = distribution,
                        PayMethod = payMethod,
                        PersonId = studentId
                    };
                    payment.PaymentItems.Add(item);
                    payments.Add(payment);
                }
            }

            return payments;
        }

        /// <summary>
        /// Build the list of payment items for a specified payment control
        /// </summary>
        /// <param name="payControl">A registration payment control</param>
        /// <param name="paymentAmount">Amount being paid</param>
        /// <param name="invoices">List of registration invoices</param>
        /// <param name="regPayments">List of registration payments</param>
        /// <param name="accountTerms">List of account terms</param>
        /// <param name="distributionMap">Mapping of receivable types to distributions</param>
        /// <param name="payReq">Payment requirement for student/term</param>
        /// <param name="receivableTypes">List of all receivable types</param>
        /// <returns>List of payment items</returns>
        private List<PaymentItem> BuildPaymentItems(RegistrationPaymentControl payControl, decimal paymentAmount, IEnumerable<Invoice> invoices,
            IEnumerable<ReceivablePayment> regPayments, List<AccountTerm> accountTerms, Dictionary<string, string> distributionMap, PaymentRequirement payReq,
            IEnumerable<ReceivableType> receivableTypes)
        {
            var paymentItems = new List<PaymentItem>();

            if (payControl.InvoiceIds != null && payControl.InvoiceIds.Count() > 0)
            {
                var arTypes = invoices.Select(x => x.ReceivableTypeCode).Distinct();
                foreach (var arType in arTypes)
                {
                    // Do we have a payment item with this invoice's AR type?
                    int pos = paymentItems.FindIndex(x => x.AccountType == arType);
                    if (pos < 0)
                    {
                        // Get the payment options for this AR type
                        var options = GetPaymentOptions(payControl, invoices, regPayments, accountTerms, distributionMap, payReq, null, new List<string>() { arType });

                        // No entry for this AR type - add it
                        var item = new PaymentItem()
                        {
                            AccountType = arType,
                            Description = receivableTypes.Where(x => x.Code == arType).FirstOrDefault().Description,
                            Term = payControl.TermId,
                            PaymentAmount = options.RegistrationBalance,
                            PaymentControlId = payControl.Id,
                            PaymentComplete = false
                        };
                        paymentItems.Add(item);
                    }
                }

                decimal overallRegBalance = paymentItems.Sum(x => x.PaymentAmount);
                decimal sumOfItemPayments = 0;

                // Set the payment amount for each payment item based on the total registration balance, registration balance for the item's AR Type, and the total payment amount
                foreach (var item in paymentItems)
                {
                    item.PaymentAmount = Math.Round((item.PaymentAmount / overallRegBalance) * paymentAmount, 2, MidpointRounding.AwayFromZero);
                    sumOfItemPayments += item.PaymentAmount;
                }

                // Ensure that individual item payment amounts add up to total payment amount, and add any disparity to the first payment item
                var calcDiff = paymentAmount - sumOfItemPayments;
                paymentItems[0].PaymentAmount += calcDiff;
            }

            // Remove any payments items with no amount to pay
            paymentItems.RemoveAll(paymentItem => paymentItem.PaymentAmount <= 0);

            return paymentItems.OrderBy(x => x.AccountType).ToList();
        }

        /// <summary>
        /// Calculates the deferral percentage available to the student
        /// </summary>
        /// <param name="applicablePaymentRequirement">The payment requirement that is applicable for the student</param>
        /// <returns>A deferral percentage</returns>
        private decimal GetDeferralPercentage(PaymentRequirement applicablePaymentRequirement)
        {
            // Find the deferral option whose date range contains the current date and return its associated deferral percentage
            if (applicablePaymentRequirement.DeferralOptions != null && applicablePaymentRequirement.DeferralOptions.Count() > 0)
            {
                foreach (var deferralOption in applicablePaymentRequirement.DeferralOptions)
                {
                    // End date specified - check if today is between the start and end dates
                    if (deferralOption.EffectiveEnd != null)
                    {
                        if (DateTime.Today.Date >= deferralOption.EffectiveStart.Date && DateTime.Today.Date <= deferralOption.EffectiveEnd.Value.Date)
                        {
                            return deferralOption.DeferralPercent;
                        }
                    }
                    // No end date specified - check if today is on or later than the start date
                    else
                    {
                        if (DateTime.Today.Date >= deferralOption.EffectiveStart.Date)
                        {
                            return deferralOption.DeferralPercent;
                        }
                    }
                }
            }

            // No applicable deferral option - 100% deferral is allowed
            return 100m;
        }

        /// <summary>
        /// Get the term balance for the student
        /// </summary>
        /// <param name="accountTerms">All account terms, independent of display selection</param>
        /// <param name="receivableTypes">Limiting receivable types</param>
        /// <param name="termId">ID of term for which balance is desired</param>
        /// <returns>Term balance for student for term for specified receivable types</returns>
        private decimal GetTermBalance(List<AccountTerm> accountTerms, IEnumerable<string> receivableTypes, string termId)
        {
            decimal termBalance = 0;
            // If the billed term is in the list of terms, get its balance
            if (accountTerms.Any(x => x.TermId == termId))
            {
                termBalance = accountTerms.Find(x => x.TermId == termId).AccountDetails.Where(y => receivableTypes.Contains(y.AccountType)).Sum(z => z.AmountDue).Value;
            }

            return termBalance;
        }

        /// <summary>
        /// Calculate the minimum payment required from the student
        /// </summary>
        /// <param name="invoiceAmount">Total amount of registration invoices</param>
        /// <param name="paymentAmount">Total amount of registration payments</param>
        /// <param name="termBalance">Balance for the term for which the student is registering</param>
        /// <param name="deferralPercentage">Deferral percentage for student</param>
        /// <returns>The minimum payment that the student must make to complete their registration</returns>
        private decimal CalculateMinimumPayment(decimal invoiceAmount, decimal paymentAmount, decimal termBalance, decimal deferralPercentage)
        {
            decimal minimumPayment = 0;

            // Non-deferred Charges = Registration Charges * (100 - Deferral Percentage), rounded to the nearest cent
            var nonDeferredCharges = Math.Round(invoiceAmount * ((100 - deferralPercentage) / 100), 2, MidpointRounding.AwayFromZero);

            // Registration Balance = Registration Charges - Registration Payments
            var registrationBalance = invoiceAmount - paymentAmount;

            // Calculated Minimum Payment = Non-deferred Charges - Registration Payments
            var calculatedMinimumPayment = nonDeferredCharges - paymentAmount;

            // Excess Payments = Greater of (Registration Balance - Term Balance) and $0
            var excessPayments = ((registrationBalance - termBalance) > 0 ? (registrationBalance - termBalance) : 0);

            // Adjusted Minimum Payment = Calculated Minimum Payment - Excess Payments
            var adjustedMinimumPayment = calculatedMinimumPayment - excessPayments;

            // Minimum Payment = Greater of (Lesser of Term Balance and Adjusted Minimum Payment) and $0
            minimumPayment = termBalance < adjustedMinimumPayment ? (termBalance > 0 ? termBalance : 0) : (adjustedMinimumPayment > 0 ? adjustedMinimumPayment : 0);

            return minimumPayment;
        }
    }
}
