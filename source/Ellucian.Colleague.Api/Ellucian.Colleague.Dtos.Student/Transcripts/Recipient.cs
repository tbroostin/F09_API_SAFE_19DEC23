using System;
using System.Xml.Serialization;

namespace Ellucian.Colleague.Dtos.Student.Transcripts
{
    /// <summary>
    /// Recipient of transcript 
    /// </summary>
    public class Recipient
    {
        /// <summary>
        /// Receiver of transcript request
        /// </summary>
        [XmlElement(Namespace = "")]
        public Receiver Receiver { get; set; }
        /// <summary>
        /// Transcript type
        /// </summary>
        [XmlElement(Namespace = "")]
        public TranscriptType TranscriptType { get; set; }
        /// <summary>
        /// Transcript purpose
        /// </summary>
        [XmlElement(Namespace = "")]
        public TranscriptPurpose TranscriptPurpose { get; set; }
        /// <summary>
        /// Delivery method
        /// </summary>
        [XmlElement(Namespace = "")]
        public DeliveryMethod DeliveryMethod { get; set; }
        /// <summary>
        /// Electronic delivery information
        /// </summary>
        [XmlElement(Namespace = "")]
        public ElectronicDelivery ElectronicDelivery { get; set; }
        
        //[XmlElement(Namespace = "")]
        //public ElectronicMethod ElectronicMethod { get; set; }   Not currently needed
        /// <summary>
        /// Indicates a request for rush processing
        /// </summary>
        [XmlElement(Namespace = "")]
        public string RushProcessingRequested { get; set; }
        /// <summary>
        /// Special delivery instructions
        /// </summary>
        [XmlElement(Namespace = "")]
        public string DeliveryInstruction { get; set; }
        /// <summary>
        /// Indicates a request for a stamped and sealed envelope
        /// </summary>
        [XmlElement(Namespace = "")]
        public string StampSealEnvelopeIndicator { get; set; }
        /// <summary>
        /// Number of copies
        /// </summary>
        [XmlElement(Namespace = "")]
        public Int32 TranscriptCopies { get; set; }
        
    }
}
