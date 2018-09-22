// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Approver DTO.
    /// </summary>
    public class Approver
    {
        /// <summary>
        /// This is the approver ID.
        /// </summary>
        public string ApproverId { get; set; }

        /// <summary>
        /// This is the name of the approver.
        /// </summary>
        public string ApprovalName { get; set; }

        /// <summary>
        /// This is the date of the approval.
        /// </summary>
        public DateTime? ApprovalDate { get; set; }
    }
}
