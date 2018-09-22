using System;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Student's birth information
    /// </summary>
    public class Birth
    {
        /// <summary>
        /// Birth Date
        /// </summary>
        [XmlElement(Namespace = "")]
        public DateTime BirthDate { get; set; }
    }
}
