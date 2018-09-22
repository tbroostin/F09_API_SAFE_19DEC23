/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// As a piece of an <see cref="AwardPackageChangeRequest"/> object, the AwardPeriodChangeRequest
    /// specifies the requested changes at the award period level.
    /// </summary>
    [Serializable]
    public class AwardPeriodChangeRequest
    {
        /// <summary>
        /// The Id of the Award Period this change request applies to
        /// </summary>
        public string AwardPeriodId { get { return awardPeriodId; } }
        private readonly string awardPeriodId;

        /// <summary>
        /// This is the requested Award Status with which to update the StudentAwardPeriod record.
        /// </summary>
        public string NewAwardStatusId { get; set; }

        /// <summary>
        /// This is the requested Amount which which to change the AwardAmount of the StudentAwardPeriod record.
        /// A null value implies that no amount update was requested
        /// </summary>
        public decimal? NewAmount { get; set; }

        /// <summary>
        /// The status of this award period request.
        /// </summary>
        public AwardPackageChangeRequestStatus Status { get; set; }

        /// <summary>
        /// A message explaining why the Status is the current status.
        /// </summary>
        public string StatusReason { get; set; }

        public AwardPeriodChangeRequest(string awardPeriodId)
        {
            if (string.IsNullOrEmpty(awardPeriodId))
            {
                throw new ArgumentNullException("awardPeriodId");
            }

            this.awardPeriodId = awardPeriodId;
        }

        /// <summary>
        /// Equals method uses the base implementation. 
        /// Since there's no reference to this AwardPeriodChangeRequest's parent AwardPackageChangeRequest,
        /// there is no way to determine equality between two AwardPeriodChangeRequests. Even though they may
        /// have the same awardPeriodId, they may have different parent AwardPackageChangeRequest.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// GetHashCode method uses the base implementation
        /// Since there's no reference to this AwardPeriodChangeRequest's parent AwardPackageChangeRequest,
        /// there is no way to determine the real HashCode of an AwardPeriodChangeRequest. Even though two AwardPeriodChangeRequests may
        /// have the same awardPeriodId, they may have different parent AwardPackageChangeRequest.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return AwardPeriodId;
        }
    }
}
