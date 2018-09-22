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
    /// StudentChecklistItem is but one item a student must complete in a given year in order to get Financial Aid.
    /// </summary>
    public class StudentChecklistItem
    {
        /// <summary>
        /// The StudentChecklistItem Code, which will match a ChecklistItem code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The ControlStatus indicates when a student must complete the checklist item 
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ChecklistItemControlStatus ControlStatus { get; set; }
    }
}
