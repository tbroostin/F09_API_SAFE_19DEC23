/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// Outside award self-reported by a student
    /// </summary>
    [Serializable]
    public class OutsideAward
    {
        /// <summary>
        /// Outside award record assigned id
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;
        /// <summary>
        /// Student id the outside award is associated with
        /// </summary>
        public string StudentId { get { return studentId; } }
        private readonly string studentId;
        /// <summary>
        /// Award year code award is associated with
        /// </summary>
        public string AwardYearCode { get { return awardYearCode; } }
        private readonly string awardYearCode;
        /// <summary>
        /// Outside award name
        /// </summary>
        public string AwardName { get { return awardName; } }
        private readonly string awardName;
        /// <summary>
        /// Outside award type: scholarship, grant, or loan
        /// </summary>
        public string AwardType { get { return awardType; } }
        private string awardType;
        /// <summary>
        /// Outside award amount
        /// </summary>
        public decimal AwardAmount { get { return awardAmount; } }
        private readonly decimal awardAmount;
        /// <summary>
        /// Outside award funding source
        /// </summary>
        public string AwardFundingSource { get { return awardFundingSource; } }
        private readonly string awardFundingSource;

        /// <summary>
        /// OutsideAward constructor without the record id argument
        /// </summary>
        /// <param name="studentId">student id award belongs to</param>
        /// <param name="awardYear">award year code award associated with</param>
        /// <param name="awardName">award name</param>
        /// <param name="awardType">award type</param>
        /// <param name="awardAmount">award amount</param>
        /// <param name="fundingSource">award funding source</param>
        public OutsideAward(string studentId, string awardYear, string awardName,
           string awardType, decimal awardAmount, string fundingSource)
        {
            if (string.IsNullOrEmpty(studentId)) { throw new ArgumentNullException("studentId"); }
            if (string.IsNullOrEmpty(awardYear)) { throw new ArgumentNullException("awardYear"); }
            if (string.IsNullOrEmpty(awardName)) { throw new ArgumentNullException("awardName"); }
            if (string.IsNullOrEmpty(awardType)) { throw new ArgumentNullException("awardType"); }
            if (awardAmount <= 0) { throw new ArgumentException("awardAmount cannot be less than or equal to 0"); }
            if (string.IsNullOrEmpty(fundingSource)) { throw new ArgumentNullException("fundingSource"); }

            this.studentId = studentId;
            this.awardYearCode = awardYear;
            this.awardName = awardName;
            this.awardType = awardType;
            this.awardAmount = awardAmount;
            this.awardFundingSource = fundingSource;
        }

        /// <summary>
        /// OutsideAward constructor with the record id argument
        /// </summary>
        /// <param name="id">outside award record id</param>
        /// <param name="studentId">student id award belongs to</param>
        /// <param name="awardYear">award year code award associated with</param>
        /// <param name="awardName">award name</param>
        /// <param name="awardType">award type</param>
        /// <param name="awardAmount">award amount</param>
        /// <param name="fundingSource">award funding source</param>
        public OutsideAward(string id, string studentId, string awardYear, string awardName, 
            string awardType, decimal awardAmount, string fundingSource)
        {
            if (string.IsNullOrEmpty(id)) { throw new ArgumentNullException("id"); }
            if (string.IsNullOrEmpty(studentId)) { throw new ArgumentNullException("studentId"); }
            if (string.IsNullOrEmpty(awardYear)) { throw new ArgumentNullException("awardYear"); }
            if (string.IsNullOrEmpty(awardName)) { throw new ArgumentNullException("awardName"); }
            if (string.IsNullOrEmpty(awardType)) { throw new ArgumentNullException("awardType"); }
            if (awardAmount <= 0) { throw new ArgumentException("awardAmount cannot be less than or equal to 0"); }
            if (string.IsNullOrEmpty(fundingSource)) { throw new ArgumentNullException("fundingSource"); }

            this.id = id;
            this.studentId = studentId;
            this.awardYearCode = awardYear;
            this.awardName = awardName;
            this.awardType = awardType;
            this.awardAmount = awardAmount;
            this.awardFundingSource = fundingSource;
        }
    }
}
