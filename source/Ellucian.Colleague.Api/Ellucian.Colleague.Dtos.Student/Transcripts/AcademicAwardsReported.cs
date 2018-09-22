using System;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Academic Awards Reported
    /// </summary>
    public class AcademicAwardsReported
    {
        /// <summary>
        /// Award Title
        /// </summary>
        [XmlElement(Namespace = "")]
        public string AcademicAwardTitle { get; set; }
        /// <summary>
        /// Award Date
        /// </summary>
        [XmlElement(Namespace = "")]
        public DateTime AcademicAwardDate { get; set; }
    }
}
