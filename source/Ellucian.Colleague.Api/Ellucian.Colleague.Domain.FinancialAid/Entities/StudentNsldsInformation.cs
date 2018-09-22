//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Student NSLDS Information class that holds NSLDS data
    /// </summary>
    [Serializable]
    public class StudentNsldsInformation
    {
        /// <summary>
        /// Student id
        /// </summary>
        public string StudentId;
        /// <summary>
        /// Pell Lifetime Eligibility used percentage
        /// </summary>
        public decimal? PellLifetimeEligibilityUsedPercentage { get { return pellLifetimeEligibilityUsedPercentage; } }
        private decimal? pellLifetimeEligibilityUsedPercentage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="pellUsedPercentage">pell used percentage</param>
        public StudentNsldsInformation(string studentId, decimal? pellUsedPercentage)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (pellUsedPercentage.HasValue && pellUsedPercentage.Value < 0)
            {
                throw new ArgumentException("pellUsedPercentage cannot be less than zero");
            }
            this.StudentId = studentId;
            this.pellLifetimeEligibilityUsedPercentage = pellUsedPercentage;
        }
    }
}
