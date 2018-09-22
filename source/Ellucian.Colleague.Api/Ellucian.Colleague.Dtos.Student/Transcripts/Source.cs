
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Source of the transcript request
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Organization
        /// </summary>
        [XmlElement(Namespace = "")]
        public Organization Organization { get; set; }
    }
}
