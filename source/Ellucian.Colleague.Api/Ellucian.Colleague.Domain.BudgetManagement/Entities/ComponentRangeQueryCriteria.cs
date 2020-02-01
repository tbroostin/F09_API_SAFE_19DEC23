// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.BudgetManagement.Entities
{
    /// <summary>
    /// Component range filter criteria.
    /// </summary>
    [Serializable]
    public class ComponentRangeQueryCriteria
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
        /// Constructor that initializes a component range query criteria domain entity.
        /// </summary>
        /// <param name="startValue">Component range query start value.</param>
        /// <param name="endValue">Component range query end value.</param>
        public ComponentRangeQueryCriteria(string startValue, string endValue)
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
