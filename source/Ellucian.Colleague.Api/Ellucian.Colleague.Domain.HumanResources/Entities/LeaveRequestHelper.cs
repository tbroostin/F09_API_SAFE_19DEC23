/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// This helper class is used for updating an existing leave request record.
    /// This class determines the list of LeaveRequestDetail records to be created/updated/deleted while updating an existing leave request.
    /// </summary>
    [Serializable]
    public class LeaveRequestHelper
    {
        /// <summary>
        /// The leaveRequest to be updated.
        /// </summary>
        public LeaveRequest LeaveRequest { get; set; }
        /// <summary>
        /// List of leave request detail records to update.
        /// </summary>
        public List<LeaveRequestDetail> LeaveRequestDetailsToUpdate { get; set; }
        /// <summary>
        /// List of leave request detail records to create.
        /// </summary>
        public List<LeaveRequestDetail> LeaveRequestDetailsToCreate { get; set; }
        /// <summary>
        /// List of leave request detail records to delete.
        /// </summary>
        public List<LeaveRequestDetail> LeaveRequestDetailsToDelete { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveRequest"></param>
        public LeaveRequestHelper(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
            {
                throw new ArgumentNullException("leaveRequest");
            }

            this.LeaveRequest = leaveRequest;
            this.LeaveRequestDetailsToUpdate = new List<LeaveRequestDetail>();
            this.LeaveRequestDetailsToCreate = new List<LeaveRequestDetail>();
            this.LeaveRequestDetailsToDelete = new List<LeaveRequestDetail>();
        }
    }
}
