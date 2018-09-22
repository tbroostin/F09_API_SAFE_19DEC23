using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// This enumeration describes the different statuses of a StudentDocument.
    /// </summary>
    public enum DocumentStatus
    {
        /// <summary>
        /// Indicates the StudentDocument has been Received
        /// </summary>
        Received,

        /// <summary>
        /// Indicates the StudentDocument has been Waived
        /// </summary>
        Waived,

        /// <summary>
        /// Indicates the StudentDocument has not been Received or Waived by the office
        /// </summary>
        Incomplete
    }
}
