﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents a tax form statement.
    /// </summary>
    [Serializable]
    public class TaxFormStatement2
    {
        /// <summary>
        /// Record ID to where the pdf data is stored
        /// </summary>
        public string PdfRecordId { get { return this.pdfRecordId; } }
        private readonly string pdfRecordId;

        /// <summary>
        /// Backing variable for PersonId.
        /// </summary>
        private readonly string personId;

        /// <summary>
        /// Person to whom this tax statement is assigned.
        /// </summary>
        public string PersonId { get { return this.personId; } }

        /// <summary>
        /// Backing variable for TaxYear.
        /// </summary>
        private readonly string taxYear;

        /// <summary>
        /// Tax year represented by this statement.
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }

        /// <summary>
        /// Backing variable for TaxForm.
        /// </summary>
        private readonly TaxForms taxForm;

        /// <summary>
        /// Type of tax form.
        /// </summary>
        public TaxForms TaxForm { get { return this.taxForm; } }

        /// <summary>
        /// Additional information to convey.
        /// </summary>
        public TaxFormNotations Notation { get; set; }

        /// <summary>
        /// Set up a tax form statement.
        /// </summary>
        /// <param name="taxYear">Tax year represented by this statement</param>
        /// <param name="notation">Name of the statement file.</param>
        /// <param name="taxform">Type of form (W-2, 1095-C, etc.)</param>
        public TaxFormStatement2(string personId, string taxYear, TaxForms taxform, string pdfRecordId)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is required.");

            if (string.IsNullOrEmpty(taxYear))
                throw new ArgumentNullException("taxYear", "taxYear is required.");

            if (string.IsNullOrEmpty(pdfRecordId))
                throw new ArgumentNullException("pdfRecordId", "pdfRecordId is required.");

            this.pdfRecordId = pdfRecordId;
            this.personId = personId;
            this.taxYear = taxYear;
            this.taxForm = taxform;
        }
    }
}
