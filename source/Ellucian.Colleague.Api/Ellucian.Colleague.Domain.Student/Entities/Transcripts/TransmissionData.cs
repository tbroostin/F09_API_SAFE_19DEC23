// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Transcripts
{
    [Serializable]
    public class TransmissionData
    {
        public string DocumentID { get; set; }              // Third party request ID plus timestamp - ignore
        public DateTime CreatedDateTime { get; set; }
        //public string DocumentTypeCode { get; set; }      // "Request"
        //public string TransmissionType { get; set; }      // "Original"
        public Source Source { get; set; }                  // Third party partner  (NSC)
        public Destination Destination { get; set; }        // Transcript producer  (Ellucian client)

        // Optional elements
        public string DocumentProcessCode { get; set; }     // Optional: TEST for test, PRODUCTION is implied
        //public string DocumentOfficialCode { get; set; }
        //public string DocumentCompleteCode { get; set; }
        public string RequestTrackingID { get; set; }       // Third party request ID
        //public UserDefinedExtensions UserDefinedExtensions { get; set; }
        public string NoteMessage { get; set; }
    }
}
