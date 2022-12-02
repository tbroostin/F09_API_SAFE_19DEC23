/* Copyright 2021 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain entity for leave request.
    /// Each leave request record may contain many leave request detail records.
    /// A leave request detail record contains the leave related details for one day.
    /// </summary>
    [Serializable]
    public class LeaveRequestDetail
    {
        #region Properties
        /// <summary>
        /// DB id of this leave request detail object
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Identifier of the leave request object
        /// </summary>
        public string LeaveRequestId { get { return leaveRequestId; } }
        private readonly string leaveRequestId;

        /// <summary>
        /// Date for which leave is requested
        /// </summary>
        public DateTime LeaveDate
        {
            get { return leaveDate; }
        }
        private DateTime leaveDate;

        /// <summary>
        /// Hours of leave requested
        /// </summary>
        public decimal? LeaveHours { get { return leaveHours; } }
        private readonly decimal? leaveHours;

        /// <summary>
        /// Indicates if this detail record has been processed in a pay period by payroll
        /// </summary>
        public bool ProcessedInPayPeriod { get { return processedInPayPeriod; } }
        private readonly bool processedInPayPeriod;

        /// <summary>
        /// Leave request detail change operator
        /// </summary>
        public string LeaveRequestDetailChgopr { get { return leaveRequestDetailChgopr; } }
        private readonly string leaveRequestDetailChgopr;

        #endregion

        #region Constructor
        /// <summary>
        ///  Parameterized contructor to instantiate a LeaveRequestDetail object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="leaveRequestId"></param>
        /// <param name="leaveDate"></param>
        /// <param name="leaveHours"></param>
        /// <param name="leaveRequestDetailChgopr"></param>
        public LeaveRequestDetail(string id,
            string leaveRequestId,
            DateTime leaveDate,
            decimal? leaveHours,
            bool processedInPayPeriod,
            string leaveRequestDetailChgopr = null)
        {
            if (leaveDate == null || leaveDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("leaveDate");
            }
            this.id = id;
            this.leaveRequestId = leaveRequestId;
            this.leaveDate = leaveDate;
            this.leaveHours = leaveHours;
            this.processedInPayPeriod = processedInPayPeriod;
            this.leaveRequestDetailChgopr = leaveRequestDetailChgopr;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Compares this object with another LeaveRequestDetail object. Two LeaveRequestDetail objects are equal when their Ids are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var leaveRequestDetail = obj as LeaveRequestDetail;

            if (leaveRequestDetail.Id == Id && leaveRequestDetail.LeaveRequestId == LeaveRequestId)
            {
                 return true;                              
            }

            return false;
        }

        /// <summary>
        /// Computes the HashCode of this object based on the Id (if present) or on LeaveRequestId, LeaveDate and LeaveHours
        /// </summary>       
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(this.Id))
            {
                return LeaveRequestId.GetHashCode() ^
                    LeaveDate.GetHashCode() ^
                    LeaveHours.GetHashCode();
            }
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// Method that determines if all properties of this object are equal to that of the input object.
        /// </summary>
        /// <param name="leaveRequestDetail"></param>
        /// <returns></returns>
        public bool CompareLeaveRequestDetail(LeaveRequestDetail leaveRequestDetail)
        {
            if (string.Equals(Id, leaveRequestDetail.Id, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(LeaveRequestId, leaveRequestDetail.LeaveRequestId, StringComparison.InvariantCultureIgnoreCase) &&
                LeaveDate == leaveRequestDetail.LeaveDate &&
                LeaveHours == leaveRequestDetail.LeaveHours)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
