using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class ProfileEFC
    {
        /// <summary>
        /// 
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? Amount { get; set; }


        public ProfileEFC(string studentId, string awardYear, int? amount)
        {
            StudentId = studentId;
            AwardYear = awardYear;
            Amount = amount;
        }
    }
}
