// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RequisiteWaiver
    {
        /// <summary>
        /// ID of the academic requirement that describes this requisite
        /// </summary>
        public string RequisiteId { get { return _requisiteId; } }
        private string _requisiteId;

        /// <summary>
        /// Indicates whether waiver has had no action, has been approved, or has been denied.
        /// </summary>
        public WaiverStatus Status { get { return _status; } }
        private WaiverStatus _status;

        /// <summary>
        /// Requisite Waiver constructor
        /// </summary>
        /// <param name="requisiteId"></param>
        /// <param name="status"></param>
        public RequisiteWaiver(string requisiteId, WaiverStatus status)
        {
            if (string.IsNullOrEmpty(requisiteId))
            {
                throw new ArgumentNullException("requisiteId", "Requisite ID is required");
            }
            if (status == null)
            {
                throw new ArgumentNullException("status", "Requisite Waiver Status is required");
            }
            _requisiteId = requisiteId;
            _status = status;
        }

        /// <summary>
        /// Two requisite waivers are equals if the Requisite Id matches.
        /// </summary>
        /// <param name="obj">Requisite Waiver object to compare to</param>
        /// <returns>boolean indicating whether equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var item = obj as RequisiteWaiver;
            return RequisiteId.Equals(item.RequisiteId);
        }

        /// <summary>
        /// Override GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.RequisiteId.GetHashCode();
        }
    }
}
