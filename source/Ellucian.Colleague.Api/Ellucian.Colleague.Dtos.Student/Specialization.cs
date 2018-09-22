using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Specialization of study
    /// </summary>
    public class Specialization
    {
        /// <summary>
        /// Code for this specialization
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description for this specialization
        /// </summary>
        public string Description { get; set; }
    }
}