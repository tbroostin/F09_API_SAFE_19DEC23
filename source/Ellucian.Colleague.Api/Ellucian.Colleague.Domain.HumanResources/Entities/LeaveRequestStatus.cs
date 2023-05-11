/* Copyright 2019-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain entity for leave request status
    /// </summary>
    [Serializable]
    public class LeaveRequestStatus
    {
        #region Properties
        /// <summary>
        /// DB id of this leave request status object
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// Identifier of the leave request object
        /// </summary>
        public string LeaveRequestId { get { return leaveRequestId; } }
        private readonly string leaveRequestId;

        /// <summary>
        /// Status of this leave request
        /// </summary>
        public LeaveStatusAction ActionType { get { return actionType; } }
        private readonly LeaveStatusAction actionType;

        /// <summary>
        /// Identifier of the actioner
        /// </summary>
        public string ActionerId { get { return actionerId; } }
        private readonly string actionerId;

        /// <summary>
        /// Name of the actioner
        /// </summary>
        public string ActionerName { get; set; }
     
        /// <summary>
        /// Timestamp metadata
        /// </summary>
        public Timestamp Timestamp { get; set; }

        /// <summary>
        /// The value of HRSS.LR.UNSUBMIT.WDRW option in LVSS form when the Leave Request is Withdrawn.
        /// </summary>
        public string WithdrawOption { get; set; }

        #endregion

        /// <summary>
        /// Parameterized contructor to instantiate a LeaveRequestStatus object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="leaveRequestId"></param>
        /// <param name="actionType"></param>
        /// /// <param name="actionerId"></param>
        public LeaveRequestStatus(string id,
            string leaveRequestId,
            LeaveStatusAction actionType,
            string actionerId)
        {
            if (string.IsNullOrEmpty(actionerId))
            {
                throw new ArgumentNullException("actionerId");
            }
            this.id = id;
            this.leaveRequestId = leaveRequestId;
            this.actionType = actionType;
            this.actionerId = actionerId;
        }
    }
}
