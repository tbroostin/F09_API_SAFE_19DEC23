// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance.Payments
{
    /// <summary>
    /// A payment receipt
    /// </summary>
    public class PaymentReceipt
    {
        /// <summary>
        /// PaymentReceipt constructor
        /// </summary>
        public PaymentReceipt()
        {
            MerchantNameAddress = new List<string>();
            ReceiptAcknowledgeText = new List<string>();
            Payments = new List<AccountsReceivablePayment>();
            Deposits = new List<AccountsReceivableDeposit>();
            OtherItems = new List<GeneralPayment>();
            ConvenienceFees = new List<ConvenienceFee>();
            PaymentMethods = new List<PaymentMethod>();
        }

        /// <summary>
        /// ID of the cash receipt
        /// </summary>
        public string CashReceiptsId { get; set; }

        /// <summary>
        /// Receipt reference number
        /// </summary>
        public string ReceiptNo { get; set; }

        /// <summary>
        /// Date the receipt was created
        /// </summary>
        public Nullable<DateTime> ReceiptDate { get; set; }

        /// <summary>
        /// Time the receipt was created
        /// </summary>
        public Nullable<DateTime> ReceiptTime { get; set; }

        /// <summary>
        /// List containing the merchant's name and address
        /// </summary>
        public List<string> MerchantNameAddress { get; set; }

        /// <summary>
        /// Merchant's contact telephone number
        /// </summary>
        public string MerchantPhone { get; set; }

        /// <summary>
        /// Merchant's contact email address
        /// </summary>
        public string MerchantEmail { get; set; }

        /// <summary>
        /// Person ID of the receipt payer
        /// </summary>
        public string ReceiptPayerId { get; set; }

        /// <summary>
        /// Name of the receipt payer
        /// </summary>
        public string ReceiptPayerName { get; set; }

        /// <summary>
        /// Acknowledgement text to include on the receipt
        /// </summary>
        public List<string> ReceiptAcknowledgeText { get; set; }

        /// <summary>
        /// URL of an image to display in the footer of the receipt
        /// </summary>
        public Uri AcknowledgeFooterImageUrl { get; set; }

        /// <summary>
        /// Text to display in the footer of the receipt
        /// </summary>
        public List<string> AcknowledgeFooterText { get; set; }

        /// <summary>
        /// Error message received while processing the payment
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// List of <see cref="AccountsReceivablePayment">account payments</see> on this receipt
        /// </summary>
        public List<AccountsReceivablePayment> Payments { get; set; }

        /// <summary>
        /// List of <see cref="AccountsReceivableDeposit">deposit payments</see> on this receipt
        /// </summary>
        public List<AccountsReceivableDeposit> Deposits { get; set; }

        /// <summary>
        /// List of <see cref="GeneralPayment">other payments</see> on this receipt
        /// </summary>
        public List<GeneralPayment> OtherItems { get; set; }

        /// <summary>
        /// List of <see cref="ConvenienceFee">convenience fees</see> paid on this receipt
        /// </summary>
        public List<ConvenienceFee> ConvenienceFees { get; set; }

        /// <summary>
        /// List of <see cref="PaymentMethod">PaymentMethod</see> on this receipt
        /// </summary>
        public List<PaymentMethod> PaymentMethods { get; set; }

        /// <summary>
        /// Return URL for workflow
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Amount of change returned 
        /// </summary>
        public decimal ChangeReturned { get; set; }
    }
}
