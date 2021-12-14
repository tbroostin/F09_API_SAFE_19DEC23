// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// criteria dto
    /// </summary>
    public class ProcurementDocumentFilterCriteria
    {
        /// <summary>
        /// person id
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// list of vendor id's
        /// </summary>
        public List<string> VendorIds { get; set; }

        /// <summary>
        /// amount range from
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// amount range to
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// date range from
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// date range to
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// list of statuses
        /// </summary>
        public List<string> Statuses { get; set; }
    }
}