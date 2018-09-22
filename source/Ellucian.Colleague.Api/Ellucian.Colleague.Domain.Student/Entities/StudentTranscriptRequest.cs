// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Class to define Transcript Request. 
    /// </summary>
    [Serializable]
    public  class StudentTranscriptRequest: StudentRequest
    {
        /// <summary>
        /// transcript grouping
        /// </summary>
        public string TranscriptGrouping { get; set;}
       /// <summary>
       /// constructor
       /// </summary>
       /// <param name="studentId"></param>
       /// <param name="recipientName"></param>
        public StudentTranscriptRequest(string studentId, string recipientName, string transcriptGrouping):base(studentId, recipientName)
        {
            this.TranscriptGrouping = transcriptGrouping;
        }

        
    }
}
