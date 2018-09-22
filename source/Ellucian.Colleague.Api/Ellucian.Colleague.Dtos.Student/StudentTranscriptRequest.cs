// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Class to define TranscriptRequest. 
    /// these are transcripts submitted from Self-Service 
    /// similar being done through TRRQ form
    /// </summary>
    public class StudentTranscriptRequest : StudentRequest
    {
        /// <summary>
        /// Transcript Grouping
        /// </summary>
        public string TranscriptGrouping { get; set; }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="recipientName"></param>
        /// <param name="MailToAddressLines"></param>
        public StudentTranscriptRequest(string studentId, string recipientName, List<string> MailToAddressLines)
            : base(studentId, recipientName, MailToAddressLines)
        {
            
        }
        /// <summary>
        /// default constructor
        /// </summary>
        public StudentTranscriptRequest():base()
        { }
    }

}

