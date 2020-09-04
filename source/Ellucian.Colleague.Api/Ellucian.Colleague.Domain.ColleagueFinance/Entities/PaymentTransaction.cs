// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class PaymentTransaction : BaseFinanceDocument
    {

        /// <summary>
        /// VOU.ECOMMERCE.SESSION and VOU.ECOMMERCE.TRANS.NO
        /// </summary>
        public string ReferenceNumber { get; set; }


        /// <summary>
        /// CDD Name: CHK.ECHECK.FLAG/VOU.AR.PAYMENT or VOU.AR.DEPOSIT.ITEMS
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// CDD Name: CHK.DATE/VOU.STATUS.DATE
        /// </summary>
        ///public DateTime? PaymentDate { get; set; }   in constructor

        /// <summary>
        /// CDD Name: CHK.VOUCHERS.IDS/VOUCHERS.ID
        /// </summary>
        //public List<string> VouchersIds { get; set; }

        /// <summary>
        /// CDD Name: CHK.AMOUNT/VOU.TOTAL.AMT
        /// </summary>
        public Decimal? PaymentAmount { get; set; }

        /// <summary>
        /// CDD Name: CHK.CURRENCY.CODE/VOU.CURRENCY.CODE
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// CDD Name: CHK.MISC.NAME/VOU.MISC.NAME 
        /// </summary>
        public List<string> MiscName { get; set; }

        /// <summary>
        /// CDD Name: CHK.ADDRESS/VOU.MISC.ADDRESS or ADDRESS.LINES
        /// </summary>
        public List<string> Address { get; set; }

        /// <summary>
        /// CDD Name: CHK.CITY/VOU.MISC.CITY 
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// CDD Name: CHK.STATE/CHK.MISC.STATE/VOU.MISC.STATE 
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// CDD Name: CHK.ZIP
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// CDD Name: CHK.COUNTRY
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// CDD Name: CHK.STATUS/VOU.STATUS
        /// </summary>
        public List<string> Status { get; set; }

        /// <summary>
        /// CDD Name: CHK.STATUS.DATE/VOU.STATUS.DATE
        /// </summary>
        public List<DateTime?> StatusDate { get; set; }

        /// <summary>
        ///  PersonCorpIndicator = "Y"
        /// </summary>
        public bool IsOrganization { get; set; }

        /// <summary>
        /// 
        /// </summary>
        //public List<string> CorpParent { get; set; }

        public bool Check { get; set; }

        public string HostCountry { get; set; }

        public string Vendor { get; set; }

        public VoucherStatus? VoucherStatus { get; set; }
        
        /// <summary>
        /// This is the private list of vouchers associated with payment transactions (checks)
        /// </summary>
        public readonly List<PaymentTransactionVoucher> Vouchers = new List<PaymentTransactionVoucher>();

        public DateTime? VoidDate { get; set; }
        
        public PaymentTransaction(string id, string guid, DateTime paymentDate)
            : base(id, guid, paymentDate)
        {
          
        } 

        /// <summary>
        /// This method adds a voucher to the payment transactions
        /// </summary>
        /// <param name="voucher">voucher object.</param>
        public void AddVoucher(PaymentTransactionVoucher voucher)
        {
            if (voucher == null)
            {
                throw new ArgumentNullException("voucher", "Voucher cannot be null");
            }
            this.Vouchers.Add(voucher);           
        }
    }
}