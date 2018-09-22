// Copyright 2017 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Status of an Organizational Person Position
    /// </summary>
    public enum OrganizationalPersonPositionStatus
    {
        /// <summary>
        /// Organizational Person Position represents a currently active role
        /// </summary>
        Current,

        /// <summary>
        /// Organizational Person Position represents a past role
        /// </summary>
        Past,
        
        /// <summary>
        /// Organizational Person Position represents a future role
        /// </summary>
        Future,

        /// <summary>
        /// Organizational Person Position has an unknown status
        /// </summary>
        Unknown
    }
}