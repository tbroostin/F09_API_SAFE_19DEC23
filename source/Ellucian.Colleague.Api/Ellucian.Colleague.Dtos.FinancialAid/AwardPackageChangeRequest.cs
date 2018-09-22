/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// AwardPackageChangeRequest object provides a way for API consumers to request
    /// updates to a student's award package. At this time, change requests can only be
    /// created to decline awards and change loan amounts. Check the student's FinancialAidConfiguration
    /// for the award year to see if change requests are required for these types of changes.
    /// </summary>
    public class AwardPackageChangeRequest
    {
        /// <summary>
        /// The database Id of the Award Change Request record
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The StudentId to whom this change request applies. 
        /// Required for POST AwardPackageChangeRequest request
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The AwardYear to which this change request applies
        /// Required for POST AwardPackageChangeRequest request
        /// </summary>
        public string AwardYearId { get; set; }

        /// <summary>
        /// The AwardId to which this change request applies
        /// Required for POST AwardPackageChangeRequest StudentAward request
        /// </summary>
        public string AwardId { get; set; }

        /// <summary>
        /// The Colleague PERSON id of the financial aid counselor to whom this request is assigned    
        /// </summary>        
        public string AssignedToCounselorId { get; set; }

        /// <summary>
        /// The date and time this Change Request was submitted and entered into the database.
        /// </summary>
        public DateTimeOffset? CreateDateTime { get; set; }

        /// <summary>
        /// This is a list of AwardPeriodChangeRequest objects. 
        /// Changes will only be requested for the award periods specified in this list.
        /// If you don't specify any period requests in this list, an exception will be thrown.
        /// In a POST AwardPackageChangeRequest request, some award period changes may be rejected outright
        /// by the system. Inspect the AwardPeriodChangeRequest's Status and StatusReason attributes for details.
        /// </summary>
        public IEnumerable<AwardPeriodChangeRequest> AwardPeriodChangeRequests { get; set; }
    }
}
