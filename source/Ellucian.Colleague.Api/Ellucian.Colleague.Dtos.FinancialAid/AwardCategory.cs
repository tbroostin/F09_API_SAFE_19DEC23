/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Financial Aid Award Category
    /// </summary>
    public class AwardCategory
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
        /// </summary>
        public string AwardCategoryType { get; set; }

    }
}
