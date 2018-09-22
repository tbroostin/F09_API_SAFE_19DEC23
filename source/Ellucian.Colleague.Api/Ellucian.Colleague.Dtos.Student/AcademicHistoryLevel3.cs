// Copyright 2017 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Academic History by Academic Level
    /// </summary>
    public class AcademicHistoryLevel3
    {
        /// <summary>
        /// Academic Level code for all credit and totals within this object
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// AcademicHistory object in which student earned credit within this academic level <see cref="AcademicHistoryBatch3"/>
        /// </summary>
        public AcademicHistoryBatch3 AcademicLevelHistory { get; set; }
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }
    }
}
