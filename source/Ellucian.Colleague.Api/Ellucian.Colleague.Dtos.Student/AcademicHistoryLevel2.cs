// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Academic History by Academic Level
    /// </summary>
    public class AcademicHistoryLevel2
    {
        /// <summary>
        /// Academic Level code for all credit and totals within this object
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// AcademicHistory object in which student earned credit within this academic level <see cref="AcademicHistoryBatch"/>
        /// </summary>
        public AcademicHistoryBatch2 AcademicLevelHistory { get; set; }
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }
    }
}
