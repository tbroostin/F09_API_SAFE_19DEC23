using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a career goal
    /// </summary>
    public class CareerGoal
    {
        /// <summary>
        /// Unique code for this career goal
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this career goal
        /// </summary>
        public string Description { get; set; }
    }
}