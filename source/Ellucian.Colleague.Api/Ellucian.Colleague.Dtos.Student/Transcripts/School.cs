using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// School
    /// </summary>
    public class School
    {
        /// <summary>
        /// Organization name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string OrganizationName { get; set; }
        /// <summary>
        /// OPE ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string OPEID { get; set; }
    }
}
