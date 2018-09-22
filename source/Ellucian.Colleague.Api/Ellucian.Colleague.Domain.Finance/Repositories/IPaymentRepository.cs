// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities.Payments;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Payment repository
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Get summary information for a payment
        /// </summary>
        /// <param name="distribution">Payment group/distribution code</param>
        /// <param name="paymentMethod">Payment method code</param>
        /// <param name="amountToPay">Amount to pay</param>
        /// <returns>Payment summary details</returns>
        PaymentConfirmation GetConfirmation(string distribution, string paymentMethod, string amountToPay);

        /// <summary>
        /// Start the processing for a credit card payment
        /// </summary>
        /// <param name="paymentDetails">Payment details</param>
        /// <returns>URL for processing the payment</returns>
        PaymentProvider StartPaymentProvider(Payment paymentDetails);

        /// <summary>
        /// Get the receipt information for an acknowledgement
        /// </summary>
        /// <param name="ecPayTransId">E-Commerce transaction ID</param>
        /// <param name="cashRcptsId">ID of cash receipt</param>
        /// <returns>Receipt acknowledgement detail</returns>
        PaymentReceipt GetCashReceipt(string ecPayTransId, string cashRcptsId);
    }
}
