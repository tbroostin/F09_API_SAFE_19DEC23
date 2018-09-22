
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Contacts
    /// </summary>
    public class Contacts
    {
        /// <summary>
        /// Address
        /// </summary>
        [XmlElement(Namespace = "")]
        public Address Address { get; set; }
        /// <summary>
        /// Phone
        /// </summary>
        [XmlElement(Namespace = "")]
        public Phone Phone { get; set; }
        /// <summary>
        /// FaxPhone
        /// </summary>
        [XmlElement(Namespace = "")]
        public Phone FaxPhone { get; set; }
    
        /// <summary>
        /// Email
        /// </summary>
        [XmlElement(Namespace = "")]
        public Email Email { get; set; }
    }
}
