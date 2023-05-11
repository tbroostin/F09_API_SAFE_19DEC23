// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FinancialAidAwardPeriod :GuidCodeItem
    {
        /// <summary>
        /// Start date of financial aid award period
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date of financial aid award period
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Status of financial aid award period
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// List of Terms associated to an award period.
        /// </summary>
        public List<string> AwardTerms { get; set; }

        /// <summary>
        /// Constructor for FinancialAidAwardPeriod
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        /// <param name="status">status</param>
        public FinancialAidAwardPeriod(string guid, string code, string description, string status)
            : base(guid, code, description)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException("Status cannot be null or empty");
            }
            this.status = status;
            AwardTerms = new List<string>();
        }
    }
}
