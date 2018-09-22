// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Cost Center component range filter criteria.
    /// </summary>
    [Serializable]
    public class CostCenterComponentRangeQueryCriteria
    {
        /// <summary>
        /// Component range start value.
        /// </summary>
        public string StartValue { get; set; }

        /// <summary>
        /// Component range end value.
        /// </summary>
        public string EndValue { get; set; }

        /// <summary>
        /// Constructor that initializes a cost center component range query criteria domain entity.
        /// </summary>
        /// <param name="startValue">Component range query start value.</param>
        /// <param name="endValue">Component range query end value.</param>
        public CostCenterComponentRangeQueryCriteria(string startValue, string endValue)
        {
            if (string.IsNullOrEmpty(startValue))
            {
                throw new ArgumentNullException("startValue", "Component range start value is a required field.");
            }
            if (string.IsNullOrEmpty(endValue))
            {
                throw new ArgumentNullException("endValue", "Component range end value is a required field.");
            }
            this.StartValue = startValue;
            this.EndValue = endValue;
        }
    }
}
