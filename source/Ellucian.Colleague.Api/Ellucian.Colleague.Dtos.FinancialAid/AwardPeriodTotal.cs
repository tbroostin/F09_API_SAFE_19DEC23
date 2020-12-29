using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// List of total amounts for each award period on an award letter
    /// </summary>
    public class AwardPeriodTotal
    {
        /// <summary>
        /// Total amount for an award period
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
