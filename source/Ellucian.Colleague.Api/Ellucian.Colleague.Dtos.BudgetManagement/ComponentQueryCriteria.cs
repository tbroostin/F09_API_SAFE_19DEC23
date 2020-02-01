// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.BudgetManagement
{
    /// <summary>
    /// Component query filter criteria.
    /// </summary>
    public class ComponentQueryCriteria
    {
        /// <summary>
        /// Component name.
        /// </summary>
        public string ComponentName { get; set; }

        /// <summary>
        /// A list of individual component filter values.
        /// </summary>
        public List<string> IndividualComponentValues { get; set; }

        /// <summary>
        /// A list of component filter range objects.
        /// </summary>
        public List<ComponentRangeQueryCriteria> RangeComponentValues { get; set; }
    }
}
