using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about the institutionally-defined topic code
    /// </summary>
    public class TopicCode
    {
        /// <summary>
        /// Unique code for this topic code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this topic code
        /// </summary>
        public string Description { get; set; }
    }
}