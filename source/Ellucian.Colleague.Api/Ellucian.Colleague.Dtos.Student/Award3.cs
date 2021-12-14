/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Financial Aid Award2 DTO
    /// </summary>
    public class Award3
    {
        /// <summary>
        /// Award2 object's Unique identifier
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Short Description of Award
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Long description of the Award2. Value in this field is contingent upon each Financial Aid Office.
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// Loan type of this award. 
        /// Null if award is not a loan.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LoanType2? LoanType { get; set; }

        /// <summary>
        /// Field for Category which can define Loan, Grant, Scholarship or Work.
        /// Null if not set up on ACD
        /// </summary>
        public AwardCategoryType? AwardCategoryType { get; set; }

        /// <summary>
        /// Field for Type which can define Funding Source:
        /// Federal, Institutional, State, Other
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Flag indicating whether this award is TIV or not
        /// </summary>
        public bool IsTitleIV { get; set; }

        /// <summary>
        /// Attribute categorizes the award for placement in the Financial Aid Shopping Sheet.
        /// If null, this award should/will not be included in the shopping sheet.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ShoppingSheetAwardGroup? ShoppingSheetGroup { get; set; }
    }
}
