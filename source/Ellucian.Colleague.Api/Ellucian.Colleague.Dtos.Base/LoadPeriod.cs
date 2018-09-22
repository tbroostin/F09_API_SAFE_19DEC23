/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Defines a given period of time to use contracts
    /// </summary>
    public class LoadPeriod
    {
        /// <summary>
        /// Id for the given loadPeriod
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description for the given load period
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Start date of the given load period (optional)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of the given load period (optional)
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
