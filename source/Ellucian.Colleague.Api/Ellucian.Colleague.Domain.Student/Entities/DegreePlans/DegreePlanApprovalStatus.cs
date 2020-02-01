// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    /// <summary>
    /// Approved: Advisor has approved the planned course item.
    /// Denied: Advisor has officially denied their approval of a particular course item.
    /// </summary>
    [Serializable]
    public enum DegreePlanApprovalStatus
    {
        Approved, Denied
    }
}
