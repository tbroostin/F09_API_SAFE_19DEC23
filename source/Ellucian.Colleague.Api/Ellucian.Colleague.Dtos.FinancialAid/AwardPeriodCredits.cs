/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// AwardPeriodCredits object contains a list of CourseCreditAssociation objects for a specific student/award year/award period combination
    /// </summary>
    public class AwardPeriodCredits
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom these credits belong
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Award Year these credits belong to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The Award Period that these credits belong to
        /// </summary>
        public string AwardPeriod { get; set; }

        /// <summary>
        /// The Award Period description 
        /// </summary>
        public string AwardPeriodDescription { get; set; }

        /// <summary>
        /// The student's program for this award period
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// Flag indicating whether Degree Audit is active for this award period
        /// </summary>
        public bool DegreeAuditActive { get; set; }
        /// <summary>
        /// List off CourseCreditAssociation records for each award period
        /// </summary>
        public System.Collections.Generic.List<CourseCreditAssociation> Coursework { get; set; }
    }
}
