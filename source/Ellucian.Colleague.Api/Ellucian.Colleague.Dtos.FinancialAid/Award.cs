/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid Award DTO
    /// </summary>
    public class Award
    {
        /// <summary>
        /// Award object's Unique identifier
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Short Description of Award
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Long description of the Award. Value in this field is contingent upon each Financial Aid Office.
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// Loan type of this award. 
        /// Null if award is not a loan.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LoanType? LoanType { get; set; }

        /// <summary>
        /// Field for Category which can define Loan, Grant, Scholarship or Work.
        /// </summary>
        public string AwardCategoryType { get; set; }

        /// <summary>
        /// Field for Type which can define Funding Source:
        /// Federal, Institutional, State, Other
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Attribute categorizes the award for placement in the Financial Aid Shopping Sheet.
        /// If null, this award should/will not be included in the shopping sheet.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ShoppingSheetAwardGroup? ShoppingSheetGroup { get; set; }
    }
}
