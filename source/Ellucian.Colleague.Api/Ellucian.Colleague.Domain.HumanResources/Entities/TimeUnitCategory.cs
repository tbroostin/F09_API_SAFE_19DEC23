/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Enumeration of Time Unit (Hour, Day, Week, Month, Year)
    /// </summary>
    [Serializable]
    public enum TimeUnitCategory
    {
        /// <summary>
        /// Hours
        /// </summary>
        Hours,

        /// <summary>
        /// Days
        /// </summary>
        Days,

        /// <summary>
        /// Weeks
        /// </summary>
        Weeks,

        /// <summary>
        /// Months
        /// </summary>
        Months,

        /// <summary>
        /// Years
        /// </summary>
        Years
    }
}
