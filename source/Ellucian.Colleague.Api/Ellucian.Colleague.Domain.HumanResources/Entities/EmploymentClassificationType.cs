using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Enumeration of possible types of an employee classification
    /// </summary>
    [Serializable]
    public enum EmploymentClassificationType
    {
        /// <summary>
        /// A classification at the position level.
        /// </summary>
        Position,
        /// <summary>
        /// A classification at the employee level.
        /// </summary>
        Employee
    }
}
