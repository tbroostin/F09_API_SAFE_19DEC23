﻿/*Copyright 2021 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    [Serializable]
    public class FinancialAidYear :GuidCodeItem
    {
        /// <summary>
        /// Start date of financial aid year
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date of financial aid year
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Host country of financial aid year
        /// </summary>
        public string HostCountry { get; set; }
        /// <summary>
        /// Status of financial aid year
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Constructor for FinancialAidYear
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        /// <param name="status">status</param>
        public FinancialAidYear(string guid, string code, string description, string status)
            : base(guid, code, description)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException("Status cannot be null or empty");
            }
            this.status = status;
        }
    }
}
