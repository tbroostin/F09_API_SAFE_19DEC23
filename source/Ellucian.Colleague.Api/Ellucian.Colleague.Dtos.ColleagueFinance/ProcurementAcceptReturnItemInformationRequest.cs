using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Detailed information for Accepting or Returning items
    /// </summary>
   public class ProcurementAcceptReturnItemInformationRequest
    {
        /// <summary>
        /// Logged in User Id
        /// </summary>
        public string StaffUserId { get; set; }

        /// <summary>
        /// Whether user accepted all the items or not
        /// </summary>
        public bool AcceptAll { get; set; }

        /// <summary>
        /// User used PO filter or not
        /// </summary>
        public bool IsPoFilterApplied { get; set; }

        /// <summary>
        /// Packing Slip
        /// </summary>
        public string PackingSlip { get; set; }

        /// <summary>
        /// Arrived Via
        /// </summary>
        public string ArrivedVia { get; set; }

        /// <summary>
        /// Item Information in detail Object
        /// </summary>
        public List<ProcurementItemInformation> ProcurementItemsInformation { get; set; }
    }
}
