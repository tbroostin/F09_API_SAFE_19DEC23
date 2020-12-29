using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a response entity of delete requisition
    /// </summary>
    [Serializable]
    public class RequisitionDeleteResponse
    {
        /// <summary>
        /// Requisition ID
        /// </summary>
        public string RequisitionId { get; set; }

        /// <summary>
        /// The Requisition number
        /// </summary>
        public string RequisitionNumber { get; set; }

        /// <summary>
        /// Flag to determine if error occured while deleting Requisition
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while deleteing Requisition
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }

    }
}
