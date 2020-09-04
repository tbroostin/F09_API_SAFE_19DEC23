using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a line item for an Procurement Receiveing Summary.
    /// </summary>
    [Serializable]

    public class RequisitionLinkSummary
    {

        /// <summary>
        /// Private system-generated id.
        /// </summary>
        private readonly string id;
        /// <summary>
        /// The requistion id
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Private name.
        /// </summary>
        private readonly string number;

        /// <summary>
        /// The requisition number
        /// </summary>
        public string Number { get { return number; } }

        public RequisitionLinkSummary(string id, string number) {
            this.id = id;
            this.number = number;
        }
    }
}
