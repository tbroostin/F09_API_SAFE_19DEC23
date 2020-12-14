// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Project line item code.
    /// (this class is moved to Colleague Finance module from Projects Accounting module)
    /// </summary>
    public class ProjectItemCode
    {
        /// <summary>
        /// Item code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Item code description
        /// </summary>
        public string Description { get; set; }
    }
}
