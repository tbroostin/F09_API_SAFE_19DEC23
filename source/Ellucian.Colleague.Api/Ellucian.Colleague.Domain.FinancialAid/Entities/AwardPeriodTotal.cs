using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid
{
    [Serializable]
    public class AwardPeriodTotal
    {
        /// <summary>
        /// Total amount for an award period
        /// </summary>
        public decimal TotalAmount { get; set; }

        public AwardPeriodTotal(decimal totalAmount)
        {
            TotalAmount = totalAmount;
        }
    }
}
