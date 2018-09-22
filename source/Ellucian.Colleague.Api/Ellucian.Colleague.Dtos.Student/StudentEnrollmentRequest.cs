// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Class to define Enrollment Verification Request. 
    /// these are enrollment verification requests submitted from Self-Service 
    /// similar being done through ENVR form
    /// </summary>
    public  class StudentEnrollmentRequest: StudentRequest
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="recipientName"></param>
        /// <param name="MailToAddressLines"></param>
        public StudentEnrollmentRequest(string studentId, string recipientName,List<string> MailToAddressLines)
            : base(studentId, recipientName,MailToAddressLines)
        {
        }
        /// <summary>
        /// default constructor
        /// </summary>
        public StudentEnrollmentRequest()
            : base()
        { }
    }
}
