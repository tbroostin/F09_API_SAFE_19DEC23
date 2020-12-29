using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Query criteria for Projects (Used by CF Module) to query api request
    /// </summary>
    [Serializable]
    public class ProjectQueryCriteria
    {
        /// <summary>
        /// General Ledger Account Ids to which projects which are associated with, to query only limited projects
        /// </summary>
        public List<string> GeneralLedgerAccountIds { get; set; }


        ///  /// <summary>
        ///  Gl Class type to which project line item to be associated with, to query only limited projects
        /// </summary>
        public GlClass? GlClass { get; set; }
    }
}
