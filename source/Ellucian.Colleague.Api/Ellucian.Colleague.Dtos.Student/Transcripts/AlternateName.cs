
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Former or Maiden name
    /// </summary>
    public class AlternateName
    {
        /// <summary>
        /// First name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string FirstName { get; set; }
        /// <summary>
        /// Middle Name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string MiddleName { get; set; }
        /// <summary>
        /// Last name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string LastName { get; set; }
    }
}
