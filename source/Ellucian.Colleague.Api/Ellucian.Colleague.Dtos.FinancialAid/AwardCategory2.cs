/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates*/
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid Award Category2
    /// </summary>
    public class AwardCategory2
    {
        /// <summary>
        /// AwardCategory object's unique identifier
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Short description of Award Category, usually used for display purposes
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Boolean flag to define if the Loan Flag is set for this category
        /// </summary>
        public bool IsLoanCategory { get; set; }

        /// <summary>
        /// Award Category type (Loan, Grant, Scholarship, or Work)
        /// null if none specified
        /// </summary>        
        public AwardCategoryType? AwardCategoryType { get; set; }

    }
}
