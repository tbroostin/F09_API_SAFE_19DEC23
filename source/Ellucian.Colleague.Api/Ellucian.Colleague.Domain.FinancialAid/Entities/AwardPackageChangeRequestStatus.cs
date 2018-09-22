using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// An enumeration of the various statuses of an AwardPackageChangeRequest
    /// </summary>
    [Serializable]
    public enum AwardPackageChangeRequestStatus
    {
        /// <summary>
        /// Indicates the change request is new and needs to be processed by the system. This is the default status
        /// </summary>
        New = 0,

        /// <summary>
        /// Indicates the change request is pending review by a Financial Aid Counselor
        /// </summary>
        Pending,

        /// <summary>
        /// Indicates the change request is accepted (either by the system or the counselor)
        /// and that the requested change was updated into the student's award package.
        /// </summary>
        Accepted,

        /// <summary>
        /// Indicates the change request was rejected by the counselor for whatever reason
        /// </summary>
        RejectedByCounselor,

        /// <summary>
        /// Indicates the change request was rejected by the system based on current business rules.
        /// </summary>
        RejectedBySystem
    }
}
