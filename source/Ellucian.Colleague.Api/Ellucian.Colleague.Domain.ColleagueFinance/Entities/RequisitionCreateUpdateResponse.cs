// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a response entity of create new requisition
    /// </summary>
    [Serializable]
    public class RequisitionCreateUpdateResponse
    {
        /// <summary>
        /// Requisition ID
        /// </summary>
        public string RequisitionId { get; set; }

        /// <summary>
        /// The requisition number
        /// </summary>
        public string RequisitionNumber { get; set; }

        /// <summary>
        /// The requisition date
        /// </summary>
        public DateTime RequisitionDate { get; set; }

        /// <summary>
        /// Flag to determine if error occured while creating requisition
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while creating requisition
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
