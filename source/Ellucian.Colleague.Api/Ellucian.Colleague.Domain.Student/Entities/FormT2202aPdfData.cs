// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FormT2202aPdfData
    {
        #region Required attributes
        /// <summary>
        /// Tax Year for the tax form.
        /// </summary>
        public string TaxYear { get { return this.taxYear; } }
        private readonly string taxYear;

        /// <summary>
        /// Student's ID
        /// </summary>
        public string StudentId { get { return this.studentId; } }
        private readonly string studentId;
        #endregion

        #region Institution attributes
        /// <summary>
        /// Institution ID used for reporting T2202As.
        /// </summary>
        public string InstitutionId { get; set; }
        /// <summary>
        /// Institution's first name and address line.
        /// </summary>
        public string InstitutionNameAddressLine1 { get; set; }

        /// <summary>
        /// Institution's second name and address line.
        /// </summary>
        public string InstitutionNameAddressLine2 { get; set; }

        /// <summary>
        /// Institution's third name and address line.
        /// </summary>
        public string InstitutionNameAddressLine3 { get; set; }
        #endregion

        #region Student attributes
        /// <summary>
        /// Student's first name and address line.
        /// </summary>
        public string StudentNameAddressLine1 { get; set; }

        /// <summary>
        /// Student's second name and address line.
        /// </summary>
        public string StudentNameAddressLine2 { get; set; }

        /// <summary>
        /// Student's third name and address line.
        /// </summary>
        public string StudentNameAddressLine3 { get; set; }

        /// <summary>
        /// Student's fourth name and address line.
        /// </summary>
        public string StudentNameAddressLine4 { get; set; }

        /// <summary>
        /// Student's fifth name and address line.
        /// </summary>
        public string StudentNameAddressLine5 { get; set; }

        /// <summary>
        /// Student's sixth name and address line.
        /// </summary>
        public string StudentNameAddressLine6 { get; set; }

        /// <summary>
        /// Student's program name.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// List of session periods that contain dates, tuition, and hours.
        /// </summary>
        public List<FormT2202aSessionPeriod> SessionPeriods = new List<FormT2202aSessionPeriod>();

        /// <summary>
        /// Student's Box A total amount.
        /// </summary>
        public string StudentBoxATotal { get { return SessionPeriods.Where(x => x.BoxAAmount.HasValue).Sum(x => x.BoxAAmount.Value).ToString("N2"); } }

        /// <summary>
        /// Student's Box B total amount.
        /// </summary>
        public int StudentBoxBTotal { get { return SessionPeriods.Where(x => x.BoxBHours.HasValue).Sum(x => x.BoxBHours.Value); } }

        /// <summary>
        /// Student's Box C total amount.
        /// </summary>
        public int StudentBoxCTotal { get { return SessionPeriods.Where(x => x.BoxCHours.HasValue).Sum(x => x.BoxCHours.Value); } }
        #endregion

        public FormT2202aPdfData(string taxYear, string studentId)
        {
            if (string.IsNullOrEmpty(taxYear))
            {
                throw new ArgumentNullException("taxYear", "Tax year is required.");
            }

            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Student ID is required.");
            }

            this.taxYear = taxYear;
            this.studentId = studentId;
        }
    }
}
