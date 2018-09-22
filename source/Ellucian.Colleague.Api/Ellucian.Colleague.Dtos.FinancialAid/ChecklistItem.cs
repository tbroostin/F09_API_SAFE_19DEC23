/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid Checklist Item
    /// </summary>
    public class ChecklistItem
    {
        /// <summary>
        /// The identifying Code of this object
        /// </summary>
        public string ChecklistItemCode { get; set; }

        /// <summary>
        /// The type of checklist item
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ChecklistItemType ChecklistItemType { get; set; }

        /// <summary>
        /// The order in which this Checklist Item should appear in a student's Financial Aid Checklist
        /// </summary>
        public int ChecklistSortNumber { get; set; }

        /// <summary>
        /// A short Description of this ChecklistItem
        /// </summary>
        public string Description { get; set; }
    }
}
