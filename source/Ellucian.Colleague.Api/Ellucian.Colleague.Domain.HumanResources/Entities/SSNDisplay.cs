/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Defines the display value of a social security number
    /// </summary>
    [Serializable]
    public enum SSNDisplay
    {
        /// <summary>
        /// All nummbers are masked or hidden
        /// </summary>
        Hidden,
        /// <summary>
        /// The last four numbers are displayed
        /// </summary>
        LastFour,
        /// <summary>
        /// All numbers are displayed
        /// </summary>
        Full
    }
}
