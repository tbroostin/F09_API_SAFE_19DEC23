/*Copyright 2015-2020 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Identifies how Awards are grouped on the financial aid shopping sheet
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShoppingSheetAwardGroup2
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
        /// Scholarships given to students by the school
        /// </summary>
        SchoolScholarships,

        /// <summary>
        /// Scholarships given to students by the state
        /// </summary>
        StateScholarships,

        /// <summary>
        /// Any other scholarships given to students
        /// </summary>
        OtherScholarships,

        /// <summary>
        /// Loans given to students by the school
        /// </summary>
        SchoolLoans,

        /// <summary>
        /// Parent PLUS Loan
        /// </summary>
        ParentPlusLoans,

        /// <summary>
        /// DlSubLoan
        /// </summary>
        DlSubLoan,

        /// <summary>
        /// DlUnsubLoan
        /// </summary>
        DlUnsubLoan,

        /// <summary>
        /// PrivateLoans
        /// </summary>
        PrivateLoans,

        /// <summary>
        /// OtherLoans
        /// </summary>
        OtherLoans,

        /// <summary>
        /// OtherJobs
        /// </summary>
        OtherJobs,

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
