//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Define the student need summary domain entity
    /// </summary>
    [Serializable]
    public class StudentNeedSummary
    {
        /// <summary>
        /// Unique identifier (GUID) for Student Need Summary (CS.ACYR)
        ///  
        /// </summary>
        public string Guid { get; private set; }

        /// <summary>
        /// The year for the student need summary.
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// The Colleague PERSON Id for this student need summary
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The federal need amount for the student/year 
        /// </summary>
        public int? FederalNeedAmount { get; set; }

        /// <summary>
        /// The institutional need amount for the student/year 
        /// </summary>
        public int? InstitutionalNeedAmount { get; set; }
       
        /// <summary>
        /// The Federal Isir Id on CS.ACYR of applicant
        /// </summary>
        public string CsFederalIsirId { get; set; }
                
        /// <summary>
        /// The Institutional Isir Id on CS.ACYR of applicant
        /// </summary>
        public string CsInstitutionalIsirId { get; set; }

        /// <summary>
        /// The GUID of the federal application outcome
        /// </summary>
        public string FederalApplicationOutcomeGuid { get; set; }

        /// <summary>
        /// The GUID of the institutional application outcome
        /// </summary>
        public string InstitutionalApplicationOutcomeGuid { get; set; }

        /// <summary>
        /// The number of months for which the budget is applicable.
        /// </summary>
        public decimal? BudgetDuration { get; set; }

        /// <summary>
        /// Federal total expenses
        /// </summary>
        public int? FederalTotalExpenses { get; set; }

        /// <summary>
        /// Institutional total expenses
        /// </summary>
        public int? InstitutionalTotalExpenses { get; set; }

        /// <summary>
        /// Federal family contribution
        /// </summary>
        public int? FederalFamilyContribution { get; set; }

        /// <summary>
        /// Institutional family contribution
        /// </summary>
        public int? InstitutionalFamilyContribution { get; set; }

        /// <summary>
        /// Federal total need reduction
        /// </summary>
        public int? FederalTotalNeedReduction { get; set; }

        /// <summary>
        /// Institutional total need reduction
        /// </summary>
        public int? InstitutionalTotalNeedReduction { get; set; }

        /// <summary>
        /// Has award(s)
        /// </summary>
        public bool HasAward { get; set; }

        /// <summary>
        /// Constructor for Student Need Summary object used for Ethos Data Model APIs.
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="guid"></param>
        public StudentNeedSummary(string studentId, string awardYear, string guid)          
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "Guid is required when creating a student need summary for student " + studentId + " for year " + awardYear);
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Stuent Id is required when creating a student need summary for Guid " + guid + " for year " + awardYear);
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear", "Award Year is required when creating a student need summary for Guid " + guid + " for student " + studentId);
            }
            Guid = guid;
            _AwardYear = awardYear;
            _StudentId = studentId;
        }
    }
}
