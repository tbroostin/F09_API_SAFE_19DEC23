/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Student NSLDS Information DTO
    /// </summary>
    public class StudentNsldsInformation
    {
        /// <summary>
        /// Student id
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// Pell lifetime eligibility used percentage
        /// </summary>
        public decimal? PellLifetimeEligibilityUsedPercentage { get; set; }
    }
}
