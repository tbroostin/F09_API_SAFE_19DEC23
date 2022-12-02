/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Time Card Type
    /// </summary>
    [Serializable]
    public enum TimecardType
    {
        /// <summary>
        /// No timecard type will be applied to the associated position
        /// </summary>
        None,
        /// <summary>
        /// Summary hours
        /// </summary>
        Summary,
        /// <summary>
        /// Detailed time
        /// </summary>
        Detail,
        /// <summary>
        /// Clock In/Clock Out
        /// </summary>
        Clock
    }
}
