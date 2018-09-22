using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Message returned as a result of the registration process
    /// </summary>
    public class RegistrationMessage
    {
        /// <summary>
        /// Unique Id of section to which this message pertains
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// Text of the message, indicating the result of the registration action
        /// </summary>
        public string Message { get; set; }
    }
}
