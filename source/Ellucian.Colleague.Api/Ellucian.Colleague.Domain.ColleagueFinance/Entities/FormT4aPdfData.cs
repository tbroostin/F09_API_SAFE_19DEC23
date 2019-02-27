// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// FormT4aPdfData
    /// </summary>
    [Serializable]
    public class FormT4aPdfData
    {
        /// <summary>
        /// Tax year for the tax form (4 digit)
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }
        private readonly string taxYear;

        /// <summary>
        /// Payer's name
        /// </summary>
        public string PayerName { get { return this.payerName; } }
        private readonly string payerName;

        /// <summary>
        ///  The Amended text
        /// </summary>
        public string Amended { get; set; }

        /// <summary>
        /// Person ID for the recipient.
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// If the recipient is a business, the recipient's account number
        /// </summary>
        public string RecipientAccountNumber { get; set; }

        /// <summary>
        /// If the recipient is an individual, the recipient's SIN
        /// </summary>
        public string Sin { get; set; }

        /// <summary>
        /// Recipient's name
        /// </summary>
        public string RecipientsName { get; set; }

        /// <summary>
        /// Recipient's first address line
        /// </summary>
        public string RecipientAddr1 { get; set; }

        /// <summary>
        /// Recipient's second address line
        /// </summary>
        public string RecipientAddr2 { get; set; }

        /// <summary>
        /// Recipient's third address line
        /// </summary>
        public string RecipientAddr3 { get; set; }

        /// <summary>
        /// Recipient's fourth address line
        /// </summary>
        public string RecipientAddr4 { get; set; }

        /// <summary>
        /// Amount for Box 16: Pension
        /// </summary>
        public decimal Pension { get; set; }

        /// <summary>
        /// Amount for Box 18: LumpSumPayment
        /// </summary>
        public decimal LumpSumPayment { get; set; }

        /// <summary>
        /// Amount for Box 20: SelfEmployedCommissions
        /// </summary>
        public decimal SelfEmployedCommissions { get; set; }

        /// <summary>
        /// Amount for Box 22: Income Tax Deducted
        /// </summary>
        public decimal IncomeTaxDeducted { get; set; }

        /// <summary>
        /// Amount for Box 24: Annuities
        /// </summary>
        public decimal Annuities { get; set; }

        /// <summary>
        /// Amount for Box 48: Fees For Services
        /// </summary>
        public decimal FeesForServices { get; set; }

        /// <summary>
        /// List of Tax form boxes with their amounts.
        /// </summary>
        public List<TaxFormBoxesPdfData> TaxFormBoxesList = new List<TaxFormBoxesPdfData>();

        /// <summary>
        /// Initializes a new instance of the T4A pdf form.
        /// </summary>
        /// <param name="taxYear">Tax year for the T4A pdf.</param>
        /// <param name="payerName">The payer's name.</param>
        public FormT4aPdfData(string taxYear, string payerName)
        {
            if (string.IsNullOrEmpty(taxYear))
            {
                throw new ArgumentNullException("taxYear", "Tax year is required.");
            }

            if (string.IsNullOrEmpty(payerName))
            {
                throw new ArgumentNullException("payerName", "Payer Name is required.");
            }

            this.taxYear = taxYear;
            this.payerName = payerName;
        }
    }
}