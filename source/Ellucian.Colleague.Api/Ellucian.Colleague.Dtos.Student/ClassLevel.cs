using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a Class Level (such as FR - Freshman)
    /// </summary>
    public class ClassLevel
    {
        /// <summary>
        /// Unique system Id for this class level
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this class level
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// sort order associated with class level
        /// </summary>
        public int? SortOrder { get; set; }
    }
}