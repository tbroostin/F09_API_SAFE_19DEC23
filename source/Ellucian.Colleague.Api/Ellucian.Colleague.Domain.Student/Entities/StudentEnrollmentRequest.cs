// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Class to define EnrollmentRequest. 
    /// these are enrollment requests submitted from Self-Service 
    /// as is being done through ENVR form
    /// </summary>
    [Serializable]
    public  class StudentEnrollmentRequest: StudentRequest
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="recipientName"></param>
        public StudentEnrollmentRequest(string studentId, string recipientName)
            : base(studentId,recipientName)
        { }

        
    }
}
