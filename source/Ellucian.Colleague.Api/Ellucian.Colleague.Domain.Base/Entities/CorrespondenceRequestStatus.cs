// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// This enumeration defines the different statuses of a CorrespondenceRequest.
    /// </summary>
    [Serializable]
    public enum CorrespondenceRequestStatus
    {
        /// <summary>
        /// Indicates the CorrespondenceRequest has been Received
        /// </summary>
        Received,

        /// <summary>
        /// Indicates the CorrespondenceRequest has been Waived
        /// </summary>
        Waived,

        /// <summary>
        /// Indicates the CorrespondenceRequest has not been Received or Waived by the office
        /// </summary>
        Incomplete
    }
}
