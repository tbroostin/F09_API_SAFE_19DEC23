using System.Xml.Serialization;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Person
    /// </summary>
    public class Person
    {
        /// <summary>
        /// School assigned person ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string SchoolAssignedPersonID { get; set; }
        /// <summary>
        /// SSN
        /// </summary>
        [XmlElement(Namespace = "")]
        public string SSN { get; set; }
        /// <summary>
        /// Birth information
        /// </summary>
        [XmlElement(Namespace = "")]
        public Birth Birth { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        [XmlElement(Namespace = "")]
        public Name Name { get; set; }
        /// <summary>
        /// Alternate name
        /// </summary>
        [XmlElement(Namespace = "")]
        public AlternateName AlternateName { get; set; }
        /// <summary>
        /// Contacts
        /// </summary>
        [XmlElement(Namespace = "")]
        public List<Contacts> Contacts { get; set; }
    }
}
