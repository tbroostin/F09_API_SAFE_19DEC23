using System.Xml.Serialization;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Requestor's receiver's organization
    /// </summary>
    public class RequestorReceiverOrganization
    {
        /// <summary>
        /// Organization name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string OrganizationName { get; set; }
        /// <summary>
        /// Contacts
        /// </summary>
        [XmlElement(Namespace = "")]
        public List<Contacts> Contacts { get; set; }
        /// <summary>
        /// OPE ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string OPEID { get; set; }
    }
}
