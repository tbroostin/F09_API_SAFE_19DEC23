/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// The definition of a component that comprises a student's Financial Aid Budget (Cost of Attendance)
    /// </summary>
    public class BudgetComponent
    {
        /// <summary>
        /// Part one of the unique identifier of this BudgetComponent object
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Part two of the unique identifier of this BudgetComponent object
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// A short description of what this BudgetComponent defines
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The group assigned to this BudgetComponent to categorize students' costs on the Financial Aid shopping sheet
        /// If null, this BudgetComponent does not get assigned to the shopping sheet
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ShoppingSheetBudgetGroup? ShoppingSheetGroup { get; set; }
    }
}
