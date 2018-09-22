/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{

    /// <summary>
    /// Defines an AverageAwardPackage that a student might receive at an institution
    /// </summary>
    public class AverageAwardPackage
    {
        /// <summary>
        /// Award year code award package is associated with
        /// </summary>
        public string AwardYearCode { get; set; }

        /// <summary>
        /// Average award amount for Grants
        /// </summary>
        public int AverageGrantAmount { get; set; }

        /// <summary>
        /// Average award amount for Loans
        /// </summary>
        public int AverageLoanAmount { get; set; }

        /// <summary>
        /// Average award amount for Scholarships
        /// </summary>
        public int AverageScholarshipAmount { get; set; }

    }
}
