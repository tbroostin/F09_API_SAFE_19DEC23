/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Provides a list of award periods to use for Requesting a Loan. It will contain
    /// either the SA.TERMS from Colleague or terms from an Attendance Pattern determined from the rules. 
    /// </summary>
    [Serializable]
    public class StudentDefaultAwardPeriod
    {
        /// <summary>
        /// The student Id associated to this record
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;
        
        /// <summary>
        /// The year for this list of award periods
        /// </summary>
        public string AwardYear { get { return awardYear; } }
        private readonly string awardYear;

        /// <summary>
        /// List of award periods for this year
        /// </summary>
        public List<string> DefaultAwardPeriods { get; set; }

        /// <summary>
        /// Constructor for StudentDefaultAwardPeriod
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student to whom this belongs.</param>
        /// <param name="awardYear">The Financial Aid award year. </param>
        public StudentDefaultAwardPeriod(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException(studentId);
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException(awardYear);
            }

            this.studentId = studentId;
            this.awardYear = awardYear;
            DefaultAwardPeriods = new List<string>();
        }
    }
}
