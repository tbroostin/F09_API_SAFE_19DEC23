using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// DTO corresponding to the elements of a PESC XML Transcript request
    /// </summary>
    [XmlRoot("TranscriptRequest", Namespace = "urn:org:pesc:message:TranscriptRequest:v1.2.0")]
    public class TranscriptRequest
    {
        /// <summary>
        /// XML Serializer namespaces
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Ellucian.StyleCop.WebApi.EllucianWebApiDtoAnalyzer", "EL1000:NoPublicFieldsOnDtos", Justification = "Already released. Risk of breaking change.")] 
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
        /// <summary>
        /// Transmission data
        /// </summary>
        public TransmissionData TransmissionData { get; set; }
        /// <summary>
        /// Document ID
        /// </summary>
        [XmlElement(Namespace = "")]
        public string DocumentID { get; set; }
        /// <summary>
        /// The body of the transcript request
        /// </summary>
        public Request Request { get; set; }
        /// <summary>
        /// Notes or messages
        /// </summary>
        [XmlElement(Namespace = "")]
        public string NoteMessage { get; set; }
        /// <summary>
        /// User defined extensions
        /// </summary>
        public UserDefinedExtensions UserDefinedExtensions { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        public TranscriptRequest()
        {
            xmlns.Add("core", "urn:org:pesc:core:CoreMain:v1.12.0");
            xmlns.Add("AcRec", "urn:org:pesc:sector:AcademicRecord:v1.0.0");
            xmlns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        }

    }
}
