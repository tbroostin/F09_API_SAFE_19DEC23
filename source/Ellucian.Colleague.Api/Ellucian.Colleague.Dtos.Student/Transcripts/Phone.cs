
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Phone
    /// </summary>
    public class Phone
    {
        /// <summary>
        /// Area code
        /// </summary>
        [XmlElement(Namespace = "")]
        public string AreaCityCode { get; set; }
        /// <summary>
        /// Number
        /// </summary>
        [XmlElement(Namespace = "")]
        public string PhoneNumber { get; set; }
    }
}
