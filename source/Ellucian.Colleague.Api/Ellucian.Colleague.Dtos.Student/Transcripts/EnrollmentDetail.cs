
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Enrollment details
    /// </summary>
    public class EnrollmentDetail
    {
        /// <summary>
        /// Program name
        /// </summary>
        [XmlElement(Namespace = "")]
        public string NameOfProgram { get; set; }
        /// <summary>
        /// Program begin year
        /// </summary>
        [XmlElement(Namespace = "")]
        public string BeginYear { get; set; }
        /// <summary>
        /// Program end year
        /// </summary>
        [XmlElement(Namespace = "")]
        public string EndYear { get; set; }
    }
}
