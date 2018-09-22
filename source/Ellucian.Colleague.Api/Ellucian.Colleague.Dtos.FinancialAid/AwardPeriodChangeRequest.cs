/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// As a piece of an <see cref="AwardPackageChangeRequest"/> object, the AwardPeriodChangeRequest
    /// specifies the requested changes at the award period level.
    /// Request status updates with the NewAwardStatusId. Currently, only denied or rejected status change requests can only be submitted.
    /// Request amount updates with NewAmount. Currently, amount change requests can only be requested for loan-type awards.
    /// </summary>
    public class AwardPeriodChangeRequest
    {
        /// <summary>
        /// The Id of the Award Period this change request applies to
        /// Required in POST AwardPackageChangeRequest request
        /// </summary>
        public string AwardPeriodId { get; set; }

        /// <summary>
        /// This is the id of the requested AwardStatus with which to change the AwardStatus of the StudentAwardPeriod record.
        /// If you leave this null or empty, no status update will be requested for this award period
        /// </summary>
        public string NewAwardStatusId { get; set; }

        /// <summary>
        /// This is the requested Amount which which to change the AwardAmount of the StudentAwardPeriod record.
        /// If you leave this null, no award amount update will be requested for this award period.
        /// </summary>
        public decimal? NewAmount { get; set; }

        /// <summary>
        /// The status of this award period request. 
        /// Requested changes may be rejected outright by the system if:
        ///     The specified awardPeriodId doesn't exist for the student.
        ///     A change request is not required for the type of change request submitted
        ///     A change request is submitted for an unsupported request type.
        /// </summary>
        public AwardPackageChangeRequestStatus Status { get; set; }

        /// <summary>
        /// A message explaining why the Status is the current status.
        /// </summary>
        public string StatusReason { get; set; }
    }
}
