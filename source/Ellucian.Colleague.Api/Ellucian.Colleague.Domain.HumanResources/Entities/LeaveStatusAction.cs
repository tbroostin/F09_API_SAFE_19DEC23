/* Copyright 2019-2022 Ellucian Company L.P. and its affiliates. */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Records the types of actions performed on a leave request.
    /// </summary>
    [Serializable]
    public enum LeaveStatusAction
    {
        /// <summary>
        /// Indicates the leave request was saved
        /// </summary>
        Draft,

        /// <summary>
        /// Indicates the leave request was submitted for approval
        /// </summary>
        Submitted,

        /// <summary>
        /// Indicates the leave request was approved
        /// </summary>
        Approved,

        /// <summary>
        /// Indicates the leave request was rejected
        /// </summary>
        Rejected,

        /// <summary>
        /// Indicates the leave request was deleted
        /// </summary>
        Deleted,

        /// <summary>
        /// Indicates the leave request was deleted
        /// </summary>
        Withdrawn,

        /// <summary>
        /// Indicates the withdraw request was rejected
        /// </summary>
        RejectWithdraw,

        /// <summary>
        /// Indicates the withdraw request was approved
        /// </summary>
        ApproveWithdraw
    }
}
