using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Derrived class of PersonNameQueryCriteria to allow for filtering out inactive employees and non-employees
    /// </summary>
    public class EmployeeNameQueryCriteria : PersonNameQueryCriteria
    {

        /// <summary>
        /// Boolean to include non-employees workers in search
        /// </summary>
        public bool IncludeNonEmployees { get; set; }

        /// <summary>
        /// Boolean to search only active users
        /// </summary>
        public bool ActiveOnly { get; set; }
    }
}
