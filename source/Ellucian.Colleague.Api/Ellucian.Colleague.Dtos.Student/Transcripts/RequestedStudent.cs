
using System.Xml.Serialization;
namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Requested student
    /// </summary>
    public class RequestedStudent
    {
        /// <summary>
        /// Information about the student's person
        /// </summary>
        [XmlElement(Namespace = "")]
        public Person Person { get; set; }
        /// <summary>
        /// Information about the student's attendance of the institution
        /// </summary>
        [XmlElement(Namespace = "")]
        public Attendance Attendance { get; set; }
    }
}
