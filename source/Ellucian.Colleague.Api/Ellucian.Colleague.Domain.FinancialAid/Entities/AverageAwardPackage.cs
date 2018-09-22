//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AverageAwardPackage describes the average award package a student might receive at an institution.
    /// </summary>
    [Serializable]
    public class AverageAwardPackage
    {
        /// <summary>
        /// Award year code average award package is associated with
        /// </summary>
        public string AwardYearCode { get { return awardYearCode; } }
        private string awardYearCode;
        /// <summary>
        /// The average amount of grant money students receive
        /// </summary>
        public int AverageGrantAmount { get { return averageGrantAmount; } }
        private int averageGrantAmount;
        /// <summary>
        /// The average amount of loan money students receive
        /// </summary>
        public int AverageLoanAmount { get { return averageLoanAmount; } }
        private int averageLoanAmount;

        /// <summary>
        /// The average amount of scholarship money the students receive.
        /// </summary>
        public int AverageScholarshipAmount { get { return averageScholarshipAmount; } }
        private int averageScholarshipAmount;

        /// <summary>
        /// Create a new AverageAwardPackage object. At least one of the constructor arguments must be non-null. Negative amounts are not allowed.
        /// Null values are converted to zero. 
        /// </summary>
        /// <param name="averageGrantAmount">The average amount of grant money</param>
        /// <param name="averageLoanAmount">the average amount of loan money</param>
        /// <param name="averageScholarshipAmount">The average amount of scholarship money</param>
        /// <exception cref="ApplicationException">Thrown if all of the constructor arguments are null</exception>
        /// <exception cref="ArgumentException">Thrown if any of the constructor arguments are less than zero</exception>
        public AverageAwardPackage(int? averageGrantAmount, int? averageLoanAmount, int? averageScholarshipAmount, string awardYearCode)
        {
            if (!averageGrantAmount.HasValue && !averageLoanAmount.HasValue && !averageScholarshipAmount.HasValue)
            {
                throw new ApplicationException("No average amounts are defined");
            }

            if (averageGrantAmount.HasValue && averageGrantAmount.Value < 0)
            {
                throw new ArgumentException("Average Grant Amount cannot be less than zero", "averageGrantAmount");
            }
            if (averageLoanAmount.HasValue && averageLoanAmount.Value < 0)
            {
                throw new ArgumentException("Average Loan Amount cannot be less than zero", "averageLoanAmount");
            }
            if (averageScholarshipAmount.HasValue && averageScholarshipAmount.Value < 0)
            {
                throw new ArgumentException("Average Scholarship Amount cannot be less than zero", "averageScholarshipAmount");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("Award year code cannot be null", "awardYearCode");
            }

            this.averageGrantAmount = (averageGrantAmount.HasValue) ? averageGrantAmount.Value : 0;
            this.averageLoanAmount = (averageLoanAmount.HasValue) ? averageLoanAmount.Value : 0;
            this.averageScholarshipAmount = (averageScholarshipAmount.HasValue) ? averageScholarshipAmount.Value : 0;
            this.awardYearCode = awardYearCode;
        }
    }
}
