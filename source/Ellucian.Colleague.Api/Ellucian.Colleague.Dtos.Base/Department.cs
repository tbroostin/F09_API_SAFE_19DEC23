using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Department code and description
    /// </summary>
    public class Department
    {
        /// <summary>
        /// Unique system code for this department
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Department description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The Division for this Department
        /// </summary>
        public string Division { get; set; }
        /// <summary>
        /// The School, such as Law School, School of Nursing, etc.
        /// </summary>
        public string School { get; set; }
    }
}
