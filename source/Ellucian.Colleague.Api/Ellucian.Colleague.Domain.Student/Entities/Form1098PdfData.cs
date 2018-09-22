// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class Form1098PdfData
    {
        #region Required attributes
        /// <summary>
        /// Tax year for the tax form
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }
        private readonly string taxYear;

        /// <summary>
        /// Federal ID of the filer/institution.
        /// </summary>
        public string InstitutionEin { get { return this.institutionEin; } }
        private readonly string institutionEin;
        #endregion

        #region Institution attributes
        /// <summary>
        /// ID of the institution.
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// Institution's name
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// Institution's first address line
        /// </summary>
        public string InstitutionAddressLine1 { get; set; }

        /// <summary>
        /// Institution's second address line
        /// </summary>
        public string InstitutionAddressLine2 { get; set; }

        /// <summary>
        /// Institution's third address line
        /// </summary>
        public string InstitutionAddressLine3 { get; set; }

        /// <summary>
        /// Institution's fourth address line
        /// </summary>
        public string InstitutionAddressLine4 { get; set; }

        /// <summary>
        /// Institution's phone number
        /// </summary>
        public string InstitutionPhoneNumber { get; set; }
        #endregion

        #region Student attributes
        /// <summary>
        /// Student's ID (required as "Service Provider/Account number)
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Student's name
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// Student's name 2
        /// </summary>
        public string StudentName2 { get; set; }

        /// <summary>
        /// Student's first address line.
        /// </summary>
        public string StudentAddressLine1 { get; set; }

        /// <summary>
        /// Student's second address line.
        /// </summary>
        public string StudentAddressLine2 { get; set; }

        /// <summary>
        /// Student's SSN
        /// </summary>
        public string SSN { get; set; }

        /// <summary>
        /// Is student at least a half-time student?
        /// </summary>
        public bool AtLeastHalfTime { get; set; }

        /// <summary>
        /// Amount billed to the student for tuition and expenses.
        /// </summary>
        public string AmountsBilledForTuitionAndExpenses { get; set; }

        /// <summary>
        /// Has the institution changed it's reporting method since becoming an Ellucian client?
        /// </summary>
        public bool ReportingMethodHasBeenChanged { get; set; }

        /// <summary>
        /// Adjustment amount for prior year.
        /// </summary>
        public string AdjustmentsForPriorYear { get; set; }

        /// <summary>
        /// Amount of scholarship/grant money received by the student.
        /// </summary>
        public string ScholarshipsOrGrants { get; set; }

        /// <summary>
        /// Adjustment amount for scholarships/grants for the prior year.
        /// </summary>
        public string AdjustmentsToScholarshipsOrGrantsForPriorYear { get; set; }

        /// <summary>
        /// Amounts billed and received for the first three months of the year.
        /// </summary>
        public bool AmountsBilledAndReceivedForQ1Period { get; set; }

        /// <summary>
        /// Is the student a graduate student.
        /// </summary>
        public bool IsGradStudent { get; set; }
        #endregion

        /// <summary>
        /// Flag for the correction checkbox
        /// </summary>
        public bool Correction { get; set; }

        /// <summary>
        /// Flag for the exclusion of loan origination fees/capilatilized interest made before Sep,2004 in box 1 of 1098E form
        /// </summary>
        public bool IsPriorInterestOrFeeExcluded { get; set; }

        /// <summary>
        /// Student Interest Amount received by the Lender
        /// </summary>
        public string StudentInterestAmount { get; set; }

        /// <summary>
        /// The current TaxForm for 1098- T or E
        /// </summary>
        public string TaxFormName { get; set; }

        public Form1098PdfData(string taxYear, string institutionEin)
        {
            if (string.IsNullOrEmpty(taxYear))
            {
                throw new ArgumentNullException("taxYear", "Tax year is required.");
            }

            if (string.IsNullOrEmpty(institutionEin))
            {
                throw new ArgumentNullException("institutionEin", "Institution EIN is required.");
            }

            this.taxYear = taxYear;
            this.institutionEin = institutionEin;
        }
    }
}