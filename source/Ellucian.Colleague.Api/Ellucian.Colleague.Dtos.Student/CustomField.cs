using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains custom field data passed from CRM to ERP.
    /// </summary>
    public class CustomField
    {
        /// <summary>
        /// Entity schema
        /// </summary>
        public String EntitySchema { get; set; }
        
        /// <summary>
        /// Attribute schema
        /// </summary>
        public String AttributeSchema { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public String Value { get; set; }
    }
}
