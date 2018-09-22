/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Identifies how Awards are grouped on the financial aid shopping sheet
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShoppingSheetAwardGroup
    {
        /// <summary>
        /// Grants given to students by the school
        /// </summary>
        SchoolGrants,

        /// <summary>
        /// Federal Pell Grants
        /// </summary>
        PellGrants,

        /// <summary>
        /// Grants given to students by the state
        /// </summary>
        StateGrants,

        /// <summary>
        /// Any other grants given to students
        /// </summary>
        OtherGrants,

        /// <summary>
        /// Awards given to students who will work for the money
        /// </summary>
        WorkStudy,

        /// <summary>
        /// Perkins Loans
        /// </summary>
        PerkinsLoans,

        /// <summary>
        /// Subsidized Federal Direct Stafford Loans
        /// </summary>
        SubsidizedLoans,

        /// <summary>
        /// Unsubsidized Federal Direct Stafford Loans
        /// </summary>
        UnsubsidizedLoans
    }
}
