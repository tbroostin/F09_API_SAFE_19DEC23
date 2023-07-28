﻿/* Copyright 2019-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO for leave request status
    /// </summary>
    public class LeaveRequestStatus
    {
        /// <summary>
        /// DB id of this leave request status object
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the leave request object
        /// </summary>
        public string LeaveRequestId { get; set; }

        /// <summary>
        /// Status of this leave request
        /// </summary>
        public LeaveStatusAction ActionType { get; set; }

        /// <summary>
        /// Identifier of the actioner
        /// </summary>
        public string ActionerId { get; set; }

        /// <summary>
        /// Name of the actioner
        /// </summary>
        public string ActionerName { get; set; }
        /// <summary>
        /// Timestamp for record add and change
        /// </summary>
        public Timestamp Timestamp { get; set; }

        /// <summary>
        /// The value of HRSS.LR.UNSUBMIT.WDRW option in LVSS form when the Leave Request is Withdrawn.
        /// </summary>
        public string WithdrawOption { get; set; }

        /// <summary>
        /// Flag denotes whether the incoming action type of the associated leave request(in the case POST/PUT), is already the latest action in the database
        /// </summary>
        public bool LatestStatusAlreadyExists { get; set; }
    }
}
