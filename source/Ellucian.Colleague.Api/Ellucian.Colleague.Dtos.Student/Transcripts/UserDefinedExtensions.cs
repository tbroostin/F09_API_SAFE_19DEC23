using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// User defined extensions
    /// </summary>
    [XmlRoot("UserDefinedExtensions", Namespace = "urn:org:pesc:message:TranscriptRequest:v1.2.0")]
    public class UserDefinedExtensions
    {
        /// <summary>
        /// Attachment URL
        /// </summary>
        [XmlElement(Namespace = "")]
        public string AttachmentURL { get; set; }
        /// <summary>
        /// Attachment flag
        /// </summary>
        [XmlElement(Namespace = "")]
        public string AttachmentFlag { get; set; }
        /// <summary>
        /// Attachment special instructions
        /// </summary>
        [XmlElement(Namespace = "")]
        public string AttachmentSpecialInstructions { get; set; }
        /// <summary>
        /// Term ID to hold request until grades are posted
        /// </summary>
        [XmlElement(Namespace = "")]
        public string HoldForTermId { get; set; }
        /// <summary>
        /// Program ID to hold request until graduation processed
        /// </summary>
        [XmlElement(Namespace = "")]
        public string HoldForProgramId { get; set; }
        /// <summary>
        /// Receiving institution FICE ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string ReceivingInstitutionFiceId { get; set; }
        /// <summary>
        /// Receiving institution CEEB ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string ReceivingInstitutionCeebId { get; set; }
        /// <summary>
        /// Unverified student ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string UnverifiedStudentId { get; set; }
        /// <summary>
        /// Enrollment details
        /// </summary>
        [XmlElement(Namespace = "")]
        public List<EnrollmentDetail> EnrollmentDetail { get; set; }
    }
}
