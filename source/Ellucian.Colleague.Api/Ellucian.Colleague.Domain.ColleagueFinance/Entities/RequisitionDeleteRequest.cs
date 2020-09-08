using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a request to delete a requisition
    /// </summary>
    [Serializable]
    public class RequisitionDeleteRequest
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Requisition ID
        /// </summary>
        public string RequisitionId { get; set; }

        /// <summary>
        ///  ConfirmationEmailAddresses
        /// </summary>
        public string ConfirmationEmailAddresses { get; set; }

    }
}
