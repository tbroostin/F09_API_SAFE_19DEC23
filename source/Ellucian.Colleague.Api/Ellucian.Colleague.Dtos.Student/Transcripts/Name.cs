using System;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Name
    /// </summary>
    public class Name
    {
        /// <summary>
        /// First name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string FirstName { get; set; }
        /// <summary>
        /// Middle name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string MiddleName { get; set; }
        /// <summary>
        /// Last Name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string LastName { get; set; }
    }
}
