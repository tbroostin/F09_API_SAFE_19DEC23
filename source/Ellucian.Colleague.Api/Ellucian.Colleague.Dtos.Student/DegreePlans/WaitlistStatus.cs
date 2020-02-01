// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
namespace Ellucian.Colleague.Dtos.Student.DegreePlans
{
    /// <summary>
    /// Enumeration contains all the possible waitlist statuses.
    /// </summary>
    public enum WaitlistStatus
    {
        /// <summary>
        /// Not currently waitlisted
        /// </summary>
        NotWaitlisted, 
        /// <summary>
        /// Actively waitlisted
        /// </summary>
        Active,
        /// <summary>
        /// Waitlisted with permission to register
        /// </summary>
        PermissionToRegister
    }
}
