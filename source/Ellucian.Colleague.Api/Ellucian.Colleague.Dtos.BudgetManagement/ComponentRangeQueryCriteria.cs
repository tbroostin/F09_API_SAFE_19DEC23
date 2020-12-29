// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Component range filter criteria.
    /// </summary>
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
    }
}
