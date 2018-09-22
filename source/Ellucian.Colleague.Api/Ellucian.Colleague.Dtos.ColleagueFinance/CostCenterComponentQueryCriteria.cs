// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Cost Center component query filter criteria.
    /// </summary>
    public class CostCenterComponentQueryCriteria
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
        public List<CostCenterComponentRangeQueryCriteria> RangeComponentValues { get; set; }
    }
}
