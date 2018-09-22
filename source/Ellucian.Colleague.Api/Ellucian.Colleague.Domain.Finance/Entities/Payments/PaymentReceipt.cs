// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.Payments
{
    [Serializable]
    public class PaymentReceipt
    {
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

        
        public string CashReceiptsId { get; set; }

        
        public string ReceiptNo { get; set; }

        
        public Nullable<DateTime> ReceiptDate { get; set; }

        
        public Nullable<DateTime> ReceiptTime { get; set; }

        
        public List<string> MerchantNameAddress { get; set; }

        
        public string MerchantPhone { get; set; }

        
        public string MerchantEmail { get; set; }

        
        public string ReceiptPayerId { get; set; }

        
        public string ReceiptPayerName { get; set; }

        
        public List<string> ReceiptAcknowledgeText { get; set; }

        
        public Uri AcknowledgeFooterImageUrl { get; set; }

        
        public List<string> AcknowledgeFooterText { get; set; }

        
        public string ErrorMessage { get; set; }

        
        public List<AccountsReceivablePayment> Payments { get; set; }

        
        public List<AccountsReceivableDeposit> Deposits { get; set; }

        
        public List<GeneralPayment> OtherItems { get; set; }

        
        public List<ConvenienceFee> ConvenienceFees { get; set; }

        
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
