/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The abstract FinancialAidApplication2 class contains attributes that all implemented classes share.
    /// </summary>
    public abstract class FinancialAidApplication2
    {
        /// <summary>
        /// The database Id of this application
        /// </summary>
        /// 
        public string Id { get; set; }

        /// <summary>
        /// The year for the application
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The Colleague PERSON Id this application applies to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The federally calculated family contribution. Will have value if application IsFederallyFlagged
        /// </summary>
        public int? FamilyContribution { get; set; }

        /// <summary>
        /// The institutionally calculated family contribution. Will have value if application IsInstitutionallyFlagged
        /// </summary>
        public int? InstitutionalFamilyContribution { get; set; }

        /// <summary>
        /// Indicates whether this application is the federally flagged application
        /// </summary>
        public bool IsFederallyFlagged { get; set; }

        /// <summary>
        /// Indicates whether this application is the institutionally flagged application
        /// </summary>
        public bool IsInstitutionallyFlagged { get; set; }
    }
}
