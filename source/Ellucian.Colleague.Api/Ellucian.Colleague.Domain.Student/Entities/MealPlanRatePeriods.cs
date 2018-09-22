// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;


namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The top level category of the admission application status type
    /// </summary>
    [Serializable]
    public enum MealPlanRatePeriods
    {
        /// <summary>
        /// Used when the value is not set or an invalid enumeration is used
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// day
        /// </summary>
        Day,
        /// <summary>
        /// week
        /// </summary>
        Week,
        /// <summary>
        /// term
        /// </summary>
        Term,
        /// <summary>
        /// meal
        /// </summary>
        Meal,
        /// <summary>
        /// month
        /// </summary>
        Month,
        /// <summary>
        /// year
        /// </summary>
        Year
    }
}
