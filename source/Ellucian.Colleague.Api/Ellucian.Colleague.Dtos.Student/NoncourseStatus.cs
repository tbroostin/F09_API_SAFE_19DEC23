using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about the institutionally-defined noncourse statuses
    /// </summary>
    public class NoncourseStatus
    {
        /// <summary>
        /// Unique code for this noncourse status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this noncourse status
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Indicates the type of status this is (accepted, notational, withdrawn)
        /// </summary>
        public NoncourseStatusType StatusType { get; set; }
    }
}
