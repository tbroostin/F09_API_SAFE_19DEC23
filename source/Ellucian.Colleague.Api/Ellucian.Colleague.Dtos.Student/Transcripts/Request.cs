
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Transcript request
    /// </summary>
    [XmlRoot("Request", Namespace = "urn:org:pesc:message:TranscriptRequest:v1.2.0")]
    public class Request
    {
        /// <summary>
        /// Requested student
        /// </summary>
        [XmlElement(Namespace = "")]
        public RequestedStudent RequestedStudent { get; set; }
        /// <summary>
        /// Recipient
        /// </summary>
        [XmlElement(Namespace = "")]
        public Recipient Recipient { get; set; }
    }
}
