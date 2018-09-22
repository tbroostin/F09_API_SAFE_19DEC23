
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Email
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Email address
        /// </summary>
        [XmlElement(Namespace = "")]
        public string EmailAddress { get; set; }
    }
}
