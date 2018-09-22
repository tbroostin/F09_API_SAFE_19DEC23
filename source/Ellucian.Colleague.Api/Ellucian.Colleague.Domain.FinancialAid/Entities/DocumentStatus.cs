//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// This enumeration defines the different statuses of a StudentDocument.
    /// </summary>
    [Serializable]
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
