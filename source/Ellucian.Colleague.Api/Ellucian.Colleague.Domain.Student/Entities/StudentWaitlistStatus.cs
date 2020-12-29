// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Individual student waitlist status detail which includes the status code- a pneumonic, status- a single character representation and status description- description of the status
    /// </summary>
    [Serializable]
    public class StudentWaitlistStatus
    {
        private string _statusCode;
        /// <summary>
        /// Waitlist status code
        /// </summary>
        public string StatusCode { get { return _statusCode; } }


        private string _status;
        /// <summary>
        /// waitlist status
        /// </summary>
        public string Status { get { return _status; } }


        private string _statusDescription;
        /// <summary>
        /// waitlist status description
        /// </summary>
        public string StatusDescription { get { return _statusDescription; } }


        /// <summary>
        /// Initialize the StudentWaitlistStatus Method
        /// </summary>
        /// <param name="statuscode">Status code- numerical representation</param>
        /// <param name="status">Status- single character </param>
        /// <param name="statusdescription">Description of the status</param>     
        public StudentWaitlistStatus(string statuscode, string status, string statusdescription)
        {
            if (string.IsNullOrEmpty(statuscode))
            {
                throw new ArgumentNullException("statuscode");
            }
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException("status");
            }
            if (string.IsNullOrEmpty(statusdescription))
            {
                throw new ArgumentNullException("statusdescription");
            }       
            _statusCode = statuscode;
            _status = status;
            _statusDescription = statusdescription;

        }


    }
}
