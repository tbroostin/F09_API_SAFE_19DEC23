﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    /// <summary>
    /// Helper class that assists in servicing requests regarding Receipts
    /// </summary>
    public static class ReceiptProcessor
    {
        /// <summary>
        /// Validates that a receipt conforms to business rules 
        /// </summary>
        /// <param name="receipt">The receipt to validate</param>
        /// <param name="payments">The list of payments to be created with this receipt</param>
        /// <param name="deposits">The list of deposits to be created with this receipt</param>
        /// <remarks>At least one payment or at least one deposit must be provided</remarks>
        /// <param name="distributions">The list of distributions against which the receipt will be validated</param>
        /// <param name="externalSystems">The list of external systems against which the receipt will be validated</param>
        /// <param name="payMethods">The list of payment methods against which the receipt will be validated</param>
        public static void ValidateReceipt(Receipt receipt, IEnumerable<ReceiptPayment> payments, IEnumerable<Deposit> deposits,
            IEnumerable<Distribution> distributions, IEnumerable<ExternalSystem> externalSystems, IEnumerable<PaymentMethod> payMethods)
        {
            if (receipt == null)
            {
                throw new ArgumentNullException("receipt", "Receipt must be specified.");
            }
            var myPayments = (payments == null ? new List<ReceiptPayment>() : payments);
            var myDeposits = (deposits == null ? new List<Deposit>() : deposits);
            if (myDeposits.Count() == 0 && myPayments.Count() == 0)
            {
                throw new ArgumentException("Payments and/or deposits must be specified.");
            }

            if (distributions == null || !distributions.Any())
            {
                throw new ArgumentNullException("distributions", "Distributions must be specified.");
            }
            if (externalSystems == null || !externalSystems.Any())
            {
                throw new ArgumentNullException("externalSystems", "External systems must be specified.");
            }
            if (payMethods == null || !payMethods.Any())
            {
                throw new ArgumentNullException("payMethods", "Valid payment methods must be specified.");
            }

            // validate the distribution
            var distr = distributions.Where(x => x.Code == receipt.DistributionCode).FirstOrDefault();
            if (distr == null)
            {
                throw new ApplicationException("Invalid receipt distribution "+receipt.DistributionCode);
            }

            // validate the external system
            if (!string.IsNullOrEmpty(receipt.ExternalSystem))
            {
                var extSys = externalSystems.Where(x => (x.Code == receipt.ExternalSystem)).FirstOrDefault();
                if (extSys ==null)
                {
                    throw new ApplicationException("Invalid external system "+receipt.ExternalSystem);
                }
            }

            // validate payment methods
            var goodPayMethCodes = from p in payMethods where p.IsValidForStudentReceivables select p.Code;
            var myPayMethCodes = from m in receipt.NonCashPayments select m.PaymentMethodCode;
            var badCodes = myPayMethCodes.Except(goodPayMethCodes);
            if (badCodes != null && badCodes.Count() != 0)
            {
                throw new ApplicationException("Invalid payment method(s) " + string.Join(", ",badCodes.ToList()));
            }

            // validate received amounts equal applied amounts
            var rcvdAmount = receipt.NonCashPayments.Sum(x => x.Amount);
            var pmtAmount = (payments == null ? 0 : payments.Sum(x => x.Amount));
            var depAmount = (deposits == null ? 0 : deposits.Sum(x => x.Amount));
            if (rcvdAmount != pmtAmount + depAmount)
            {
                throw new ArgumentException("Amounts received on a receipt must match the amount of payments plus the amount of deposits.");
            }
        }

        /// <summary>
        /// Perform additional validations specific to Receipts generated by external Residence Life systems
        /// </summary>
        /// <param name="receipt">The receipt to validate</param>
        /// <param name="deposits">The list deposits to be associated with this receipt</param>
        /// <param name="cashier">The cashier to be associated with this receipt</param>
        public static void ValidateResidenceLifeReceipt(Receipt receipt, IEnumerable<Deposit> deposits, Cashier cashier)
        {
            // receipt is required
            if (receipt == null)
            {
                throw new ArgumentNullException("receipt", "Receipt must be specified.");
            }

            // deposits to be created must be specified
            if (deposits == null)
            {
                throw new ArgumentNullException("deposits", "Deposits must be specified.");
            }

            // cashier must be specified
            if (cashier == null)
            {
                throw new ArgumentNullException("cashier", "Cashier must be specified.");
            }

            // payer name must be specified
            if (string.IsNullOrEmpty(receipt.PayerName)) {
                throw new ArgumentException("Payer name must be specified.");
            }

            // cashier not ecommerce enabled
            if (cashier.IsECommerceEnabled)
            {
                throw new ArgumentException("The cashier on the receipt cannot be e-commerce enabled.");
            }

            // date not in future
            if (receipt.Date.Date > DateTime.Now.Date)
            {
                throw new ArgumentException("the receipt date may not be in the future.");
            }

            // exactly 1 non-cash payment
            if (receipt.NonCashPayments.Count != 1)
            {
                throw new ArgumentException("Exactly one non-cash payment must be specified.");
            }

            // exactly 1 deposit
            if (deposits.Count() != 1)
            {
                throw new ArgumentException("Exactly one deposit must be specified.");
            }
            
            // external information must be specified
            if (string.IsNullOrEmpty(receipt.ExternalIdentifier) || string.IsNullOrEmpty(receipt.ExternalSystem))
            {
                throw new ApplicationException("The external system and external identifier must be specified.");
            }
        }
    }
}
