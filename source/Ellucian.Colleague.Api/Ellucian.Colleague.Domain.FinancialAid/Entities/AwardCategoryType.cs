using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// An AwardCategoryType generalizes groups of awards even further than 
    /// an award's category.
    /// </summary>
    [Serializable]
    public enum AwardCategoryType
    {
        Loan,
        Grant,
        Scholarship,
        Work
    }
}
