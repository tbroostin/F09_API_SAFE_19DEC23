// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This enumeration describes the different statuses of a CorrespondenceRequest.
    /// </summary>
    public enum CorrespondenceRequestStatus
    {
        /// <summary>
        /// Indicates the CorrespondenceRequest has been Received
        /// </summary>
        Received,

        /// <summary>
        /// Indicates the CorrespondenceRequest  has been Waived
        /// </summary>
        Waived,

        /// <summary>
        /// Indicates the CorrespondenceRequest has not been Received or Waived by the office
        /// </summary>
        Incomplete
    }

}
