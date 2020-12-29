// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Represents a tax form statement.
    /// Third version of TaxFormStatement because of the addition of the 1099-NEC
    /// tax form to the list of tax forms. Instead of using the TaxForms DTO, we
    /// moved to TaxFormTypes in the domain so, if new forms are added, the APIs
    /// do not have to be versioned again.
    /// </summary>
    [Serializable]
    public class TaxFormStatement3
    {
        /// <summary>
        /// Record ID to where the pdf data is stored
        /// </summary>
        public string PdfRecordId { get { return this.pdfRecordId; } }
        private readonly string pdfRecordId;

        /// <summary>
        /// Person to whom this tax statement is assigned.
        /// </summary>
        public string PersonId { get { return this.personId; } }
        private readonly string personId;

        /// <summary>
        /// Tax year represented by this statement.
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }
        private readonly string taxYear;

        /// <summary>
        /// Type of tax form.
        /// </summary>
        public string TaxForm { get { return this.taxForm; } }
        private string taxForm;

        /// <summary>
        /// Additional information to convey.
        /// </summary>
        public TaxFormNotations Notation { get; set; }

        /// <summary>
        /// Id of state representing the current tax form.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Date of when the record was created.
        /// </summary>
        public DateTime? AddDate { get { return addDate; } }

        /// <summary>
        /// Backing variable for TaxForm.
        /// </summary>
        private readonly DateTime? addDate;

        /// <summary>
        /// Set up a tax form statement.
        /// </summary>
        /// <param name="taxYear">Tax year represented by this statement</param>
        /// <param name="notation">Name of the statement file.</param>
        /// <param name="taxform">Type of form (W-2, 1095-C, etc.)</param>
        public TaxFormStatement3(string personId, string taxYear, string taxform, string pdfRecordId)
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

        /// <summary>
        /// Set up a tax form statement.
        /// </summary>
        /// <param name="taxYear">Tax year represented by this statement</param>
        /// <param name="notation">Name of the statement file.</param>
        /// <param name="taxform">Type of form (W-2, 1095-C, etc.)</param>
        public TaxFormStatement3(string personId, string taxYear, string taxform, string pdfRecordId, DateTime? addDate)
        {
            if (string.IsNullOrEmpty(personId))
                throw new ArgumentNullException("personId", "personId is required.");

            if (string.IsNullOrEmpty(taxYear))
                throw new ArgumentNullException("taxYear", "taxYear is required.");

            if (string.IsNullOrEmpty(pdfRecordId))
                throw new ArgumentNullException("pdfRecordId", "pdfRecordId is required.");
            if (!addDate.HasValue)
                throw new ArgumentNullException("addDate", "addDate is required");

            this.pdfRecordId = pdfRecordId;
            this.personId = personId;
            this.taxYear = taxYear;
            this.taxForm = taxform;
            this.addDate = addDate.Value;
        }

        /// <summary>
        /// Set up a tax form statement.
        /// </summary>
        /// <param name="taxYear">Tax year represented by this statement</param>
        /// <param name="notation">Name of the statement file.</param>
        /// <param name="taxform">Type of form (W-2, 1095-C, etc.)</param>
        /// <param name="state">State Id of the current tax form</param>
        public TaxFormStatement3(string personId, string taxYear, string taxform, string pdfRecordId, string state)
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
            this.State = state;
        }
    }
}
