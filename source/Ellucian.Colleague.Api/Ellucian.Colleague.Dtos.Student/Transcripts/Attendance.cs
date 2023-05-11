// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Information on school attended
    /// </summary>
    
    public class Attendance
    {
        /// <summary>
        /// School
        /// </summary>
        [XmlElement(Namespace = "")]
        public School School { get; set; }
        /// <summary>
        /// Date of Enrollment
        /// </summary>
        [XmlElement(Namespace = "")]
        public DateTime? EnrollDate { get; set; }
        /// <summary>
        /// Date of exit
        /// </summary>
        [XmlElement(Namespace = "")]
        public DateTime? ExitDate { get; set; }
        /// <summary>
        /// Indicates current enrollment
        /// </summary>
        [XmlElement(Namespace = "")]
        public string CurrentEnrollmentIndicator { get; set; }
        /// <summary>
        /// Degrees or other awards reported
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")]
        [XmlElement(Namespace = "")]
        public List<AcademicAwardsReported> AcademicAwardsReported { get { return academicAwardsReported; } set { academicAwardsReported = value; } }
        private List<AcademicAwardsReported> academicAwardsReported = new List<AcademicAwardsReported>();
    }
}
