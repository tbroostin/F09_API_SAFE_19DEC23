
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Receiver of transcript request
    /// </summary>
    public class Receiver
    {
        /// <summary>
        /// Requestor receiver organization
        /// </summary>
        [XmlElement(Namespace = "")]
        public RequestorReceiverOrganization RequestorReceiverOrganization { get; set; }
    }
}
