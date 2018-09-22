//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Award Period DTO
    /// </summary>
    public class AwardPeriod
    {
        /// <summary>
        /// Award Period Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the Award Period
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Starting Date of the Award Period
        /// </summary>
        public DateTime StartDate { get; set; }
    }
}
