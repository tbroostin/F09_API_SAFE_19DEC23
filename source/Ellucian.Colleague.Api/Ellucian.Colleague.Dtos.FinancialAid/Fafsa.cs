/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Define the FAFSA data set
    /// </summary>
    public class Fafsa : FinancialAidApplication2
    {
        /// <summary>
        /// True or False, is the student Pell Eligible
        /// </summary>
        public bool IsPellEligible { get; set; }
        /// <summary>
        /// Parents Adjusted Gross Income
        /// </summary>
        public int? ParentsAdjustedGrossIncome { get; set; }
        /// <summary>
        /// Students Adjusted Gross Income
        /// </summary>
        public int? StudentsAdjustedGrossIncome { get; set; }
    }
}
