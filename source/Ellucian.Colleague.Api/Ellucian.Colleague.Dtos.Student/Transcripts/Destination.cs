using System;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Destination of transcript request
    /// </summary>
    public class Destination
    {
        /// <summary>
        /// Organization
        /// </summary>
        [XmlElement(Namespace = "")]
        public Organization Organization { get; set; }
    }
}
