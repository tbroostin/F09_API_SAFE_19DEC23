// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class Recipient
    {
        public Receiver Receiver { get; set; }
        public TranscriptType TranscriptType { get; set; }
        public TranscriptPurpose TranscriptPurpose { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; }
        public ElectronicDelivery ElectronicDelivery { get; set; }
        public string RushProcessingRequested { get; set; }
        public string DeliveryInstruction { get; set; }
        public string StampSealEnvelopeIndicator { get; set; }
        public Int32 TranscriptCopies { get; set; }
        
    }
}
