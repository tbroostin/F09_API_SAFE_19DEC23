using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All information related to Recruiter test connection
    /// </summary>
    public class ConnectionStatus
    {
        /// <summary>
        /// Response Service URL
        /// </summary>
        public string ResponseServiceURL { get; set; }

        /// <summary>
        /// Connection error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Duration
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// Success boolean
        /// </summary>
        public string Success { get; set; }

        /// <summary>
        /// CRM organization name
        /// </summary>
        public string RecruiterOrganizationName { get; set; }

        /// <summary>
        /// CRM organization GUID
        /// </summary>
        public string RecruiterOrganizationId { get; set; }
    }
}
