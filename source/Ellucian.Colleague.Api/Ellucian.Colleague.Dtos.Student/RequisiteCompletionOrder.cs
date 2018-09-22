using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Possible values indicating when a requisite must be completed in relation to the course.
    /// </summary>
    public enum RequisiteCompletionOrder
    {
        /// <summary>
        /// The requisite must be completed at the same time as the item.
        /// </summary>
        Concurrent, 
        /// <summary>
        /// The requisite must be completed prior to taking the item.
        /// </summary>
        Previous, 
        /// <summary>
        /// The requisite can be taken either prior to or at the same time as the item.
        /// </summary>
        PreviousOrConcurrent
    }
}
