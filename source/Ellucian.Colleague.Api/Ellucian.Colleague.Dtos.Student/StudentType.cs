using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Type such as in-state or out-of-state.
    /// </summary>
    public class StudentType
    {
        /// <summary>
        /// Code for this student type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this student type
        /// </summary>
        public string Description { get; set; }
    }
}
