/*Copyright 2018 Ellucian Company L.P. and its affiliates*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    /// <summary>
    /// Holds disbursement information about a particular student award
    /// </summary>
    [Serializable]
    public class StudentAwardDisbursementInfo
    {
        /// <summary>
        /// Student id the award belongs to
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;
        /// <summary>
        /// Award Code
        /// </summary>
        public string AwardCode { get { return awardCode; } }
        private readonly string awardCode;
        /// <summary>
        /// Award Description
        /// </summary>
        public string AwardDescription { get; set; }
        /// <summary>
        /// Award year code
        /// </summary>
        public string AwardYearCode { get { return awardYearCode; } }
        private readonly string awardYearCode;
        /// <summary>
        /// List of Award Disbursements
        /// </summary>
        public List<StudentAwardDisbursement> AwardDisbursements { get; set; }

        /// <summary>
        /// StudentAwardDisbursementInfo constructor
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardCode"></param>
        /// <param name="awardYearCode"></param>
        public StudentAwardDisbursementInfo(string studentId, string awardCode, string awardYearCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardCode))
            {
                throw new ArgumentNullException("awardCode");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }

            this.studentId = studentId;
            this.awardCode = awardCode;
            this.awardYearCode = awardYearCode;
            this.AwardDisbursements = new List<StudentAwardDisbursement>();
        }
    }
}
