// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Individual student waitlist status detail which includes the status code- a pneumonic, status- character representation and status description- description of the status
    /// </summary>
    public class StudentWaitlistStatus
    {
        /// <summary>
        /// Waitlist status code
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// waitlist status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// waitlist status description
        /// </summary>
        public string StatusDescription { get; set; }

    }
}

