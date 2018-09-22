//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid Award Year DTO
    /// </summary>
    public class AwardYear
    {
        /// <summary>
        /// AwardYear object's unique identifier
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Short description of Award Year, usually used for display purposes
        /// </summary>
        public string Description { get; set; }
    }
}
