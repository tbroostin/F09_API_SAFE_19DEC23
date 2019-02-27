// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Form1099MIPdfData
    /// </summary>
    [Serializable]
    public class Form1099MIPdfData
    {
        /// <summary>
        /// Tax year for the tax form (4 digit)
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }
        private readonly string taxYear;
    
        #region Payer attributes

        /// <summary>
        /// ID of the Payer.
        /// </summary>
        public string PayerId { get; set; }

        /// <summary>
        /// Payer's name
        /// </summary>
        public string PayerName { get { return this.payerName; } }
        private readonly string payerName;

        /// <summary>
        /// Payer's first address line
        /// </summary>
        public string PayerAddressLine1 { get; set; }

        /// <summary>
        /// Payer's second address line
        /// </summary>
        public string PayerAddressLine2 { get; set; }

        /// <summary>
        /// Payer's third address line
        /// </summary>
        public string PayerAddressLine3 { get; set; }

        /// <summary>
        /// Payer's fourth address line
        /// </summary>
        public string PayerAddressLine4 { get; set; }

        /// <summary>
        /// Payer's phone number
        /// </summary>
        public string PayerPhoneNumber { get; set; }

        /// <summary>
        /// If the payer is an institution, the payer's EIN
        /// </summary>
        public string PayersEin { get; set; }
        #endregion

        #region Recipient attributes

        /// <summary>
        /// Person ID for the recipient.
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// If the recipient is a business, the recipient's account number
        /// </summary>
        public string RecipientAccountNumber { get; set; }

        /// <summary>
        /// If the recipient is an individual, the recipient's EIN
        /// </summary>
        public string Ein { get; set; }

        /// <summary>
        /// Account Number is PersonId*State combination
        /// </summary>
        public string AccountNumber
        {
            get { return this.RecipientAccountNumber + "*" + this.State; }
        }

        /// <summary>
        /// Recipient's name
        /// </summary>
        public string RecipientsName { get; set; }
        /// <summary>
        /// Recipient's SecondName
        /// </summary>
        public string RecipientSecondName { get; set; }

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

        #endregion

        /// <summary>
        /// Correction flag in case the latest had a verified version after correction
        /// </summary>
        public bool IsCorrected { get; set; }

        /// <summary>
        /// List of Tax form boxes with their amounts.
        /// </summary>
        public List<TaxFormBoxesPdfData> TaxFormBoxesList = new List<TaxFormBoxesPdfData>();


        /// <summary>
        /// State Code for the TaxForm
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// State/Payer's State Number
        /// </summary>
        public string StatePayerNumber { get; set; }

        /// <summary>
        /// Direct Resale
        /// </summary>
        public bool IsDirectResale { get; set; }

        /// <summary>
        /// Box Country
        /// </summary>
        public string BoxCountry { get; set; }

        /// <summary>
        /// Foreign Tax Paid Amount (Required for Tax year 2013)
        /// </summary>
        public string ForeignTaxPaid { get; set; }

        /// <summary>
        /// Initializes a new instance of the 1099MI pdf form.
        /// </summary>
        /// <param name="taxYear">Tax year for the 1099MI pdf.</param>
        /// <param name="payerName">The payer's name.</param>
        public Form1099MIPdfData(string taxYear, string payerName)
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