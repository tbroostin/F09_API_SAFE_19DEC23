using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a response to accept/return items
    /// </summary>
    [Serializable]
    public class ProcurementAcceptReturnItemInformationResponse
    {
        /// <summary>
        /// Error Occurred or not
        /// </summary>
        public bool ErrorOccurred { get; set; }
        /// <summary>
        /// Error Messages
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Warning messages exists or not
        /// </summary>
        public bool WarningOccurred { get; set; }

        /// <summary>
        /// Warning Messages
        /// </summary>
        public List<string> WarningMessages { get; set; }
        
        /// <summary>
        /// Item Information in detail Object
        /// </summary>
        public List<ProcurementItemInformationResponse> ProcurementItemsInformationResponse { get; set; }

        public ProcurementAcceptReturnItemInformationResponse() {
            ProcurementItemsInformationResponse = new List<ProcurementItemInformationResponse>();
        }

    }
}
