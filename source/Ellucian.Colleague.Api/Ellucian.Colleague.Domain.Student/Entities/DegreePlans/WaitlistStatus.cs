// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    /// <summary>
    /// Status of a person on a waitlist - related to the planned course on the plan.
    /// </summary>
    [Serializable]
    public enum WaitlistStatus
    {
        NotWaitlisted, Active, PermissionToRegister
    }
}
