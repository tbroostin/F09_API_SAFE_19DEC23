// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Cost Center component range filter criteria.
    /// </summary>
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
    }
}
