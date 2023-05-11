// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Dtos.Finance.Payments;

namespace Ellucian.Colleague.Coordination.Finance
{
    public interface IPaymentService
    {
        /// <summary>
        /// Get the demographic information for a payer
        /// </summary>
        /// <param name="personId">Payer ID</param>
        /// <returns>Demographic details</returns>
        ElectronicCheckPayer GetCheckPayerInformation(string personId);

        /// <summary>
        /// Get the payment information for a planned payment
        /// </summary>
        /// <param name="distribution">Payment group/distribution code</param>
        /// <param name="paymentMethod">Payment method code</param>
        /// <param name="amountToPay">Amount to pay on account</param>
        /// <returns>Payment processing details</returns>
        PaymentConfirmation GetPaymentConfirmation(string distribution, string paymentMethod, string amountToPay);

        /// <summary>
        /// Get a payment receipt
        /// </summary>
        /// <param name="transactionId">E-Commerce transaction ID</param>
        /// <param name="cashReceiptId">Cash receipts ID</param>
        /// <returns>Detailed receipt information</returns>
        PaymentReceipt GetPaymentReceipt(string transactionId, string cashReceiptId);

        /// <summary>
        /// Start processing a credit card payment
        /// </summary>
        /// <param name="paymentDetails">Payment details</param>
        /// <returns>Payment gateway URL</returns>
        PaymentProvider PostPaymentProvider(Payment paymentDetails);

        /// <summary>
        /// Process an e-check
        /// </summary>
        /// <param name="paymentDetails">Payment details</param>
        /// <returns>Result of e-check processing</returns>
        ElectronicCheckProcessingResult PostProcessElectronicCheck(Payment paymentDetails);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="allPaymentMethods"></param>
        /// <returns></returns>
        Task<IEnumerable<Ellucian.Colleague.Dtos.Finance.Configuration.AvailablePaymentMethod>> GetRestrictedPaymentMethodsAsync(string studentId, IEnumerable<AvailablePaymentMethod> allPaymentMethods);
    }
}
