// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible workflow and worklist execution states
    /// </summary>
    [Serializable]
    public enum ExecutionState
    {
        /// <summary>
        /// Workflow or worklist has been opened but not started
        /// </summary>
        NS,

        /// <summary>
        /// Workflow or worklist has been opened but is not active
        /// </summary>
        ON,

        /// <summary>
        /// Workflow or worklist has been completed and closed
        /// </summary>
        C
    }
}
