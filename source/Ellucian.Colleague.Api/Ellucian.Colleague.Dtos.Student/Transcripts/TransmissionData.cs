using System;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Information about the transcript request
    /// </summary>
    [XmlRoot("TransmissionData", Namespace = "urn:org:pesc:message:TranscriptRequest:v1.2.0")]
    public class TransmissionData
    {
        /// <summary>
        /// Document ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string DocumentID { get; set; }              // Third party request ID plus timestamp - ignore
        /// <summary>
        /// Date and time of request creation
        /// </summary>
        [XmlElement(Namespace = "")]
        public DateTime CreatedDateTime { get; set; }
        //public string DocumentTypeCode { get; set; }      // "Request"
        //public string TransmissionType { get; set; }      // "Original"
        /// <summary>
        /// Source
        /// </summary>
        [XmlElement(Namespace = "")]
        public Source Source { get; set; }                  // Third party partner  (NSC)
        /// <summary>
        /// Destination
        /// </summary>
        [XmlElement(Namespace = "")]
        public Destination Destination { get; set; }        // Transcript producer  (Ellucian client)

        // Optional elements
        /// <summary>
        /// Document process code
        /// </summary>
        [XmlElement(Namespace = "")]
        public string DocumentProcessCode { get; set; }     // Optional: TEST for test, PRODUCTION is implied
        //public string DocumentOfficialCode { get; set; }
        //public string DocumentCompleteCode { get; set; }
        /// <summary>
        /// Request tracking ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string RequestTrackingID { get; set; }       // Third party request ID
        //public UserDefinedExtensions UserDefinedExtensions { get; set; }
        /// <summary>
        /// Notes or messages
        /// </summary>
        [XmlElement(Namespace = "")]
        public string NoteMessage { get; set; }
    }
}
