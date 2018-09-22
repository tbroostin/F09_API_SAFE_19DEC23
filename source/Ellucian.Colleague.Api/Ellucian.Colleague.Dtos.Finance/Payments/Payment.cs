// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// A payment to be made
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Payment constructor
        /// </summary>
        public Payment()
        {
            PaymentItems = new List<PaymentItem>();
        }

        /// <summary>
        /// ID of account for which payment is made
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// ID of the user making payment
        /// </summary>        
        public string PayerId { get; set; }

        /// <summary>
        /// URL to return to when payment is complete
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Distribution code for which payment is made
        /// </summary>
        public string Distribution { get; set; }

        /// <summary>
        /// Method of payment code
        /// </summary>
        public string PayMethod { get; set; }

        /// <summary>
        /// Amount to pay
        /// </summary>
        public decimal AmountToPay { get; set; }

        /// <summary>
        /// ID of e-commerce account to use for payment
        /// </summary>
        public string ProviderAccount { get; set; }

        /// <summary>
        /// Code of convenience fee to charge
        /// </summary>
        public string ConvenienceFee { get; set; }

        /// <summary>
        /// Amount of convenience fee to charge
        /// </summary>
        public decimal? ConvenienceFeeAmount { get; set; }

        /// <summary>
        /// GL number of convenience fee to charge
        /// </summary>
        public string ConvenienceFeeGeneralLedgerNumber { get; set; }

        /// <summary>
        /// Details needed to process an e-check
        /// </summary>
        public CheckPayment CheckDetails { get; set; }

        /// <summary>
        /// List of <see cref="PaymentItem">payment items</see> to be paid
        /// </summary>
        public List<PaymentItem> PaymentItems { get; set; }

        /// <summary>
        /// URL to return to after making an IPC payment
        /// </summary>
        public string ReturnToOriginUrl { get; set; }
    }
}
