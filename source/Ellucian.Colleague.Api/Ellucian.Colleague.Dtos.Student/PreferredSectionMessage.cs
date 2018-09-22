// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Message returned from Preferred Section processing
    /// </summary>
    public class PreferredSectionMessage
    {
        /// <summary>
        /// Unique ID of the section to which this message pertains, if applicable
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// Text of the message, indicating the result of the attempted actions
        /// </summary>
        public string Message { get; set; }
    }
}
