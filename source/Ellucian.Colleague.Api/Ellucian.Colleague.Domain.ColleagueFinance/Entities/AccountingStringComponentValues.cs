// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents the values needed for accounting String Component Values which is
    /// retrieved from both the GL and Projects entities
    /// </summary>
    [Serializable]
    public class AccountingStringComponentValues
    {
        /// <summary>
        /// Represents the GL or Project ID 
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// GUID
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Record key
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Description of the GL/Project
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Can either be GL or Project
        /// </summary>
        public string AccountDef { get; set; }

        /// <summary>
        /// Get the values of the Status for either GL/Project
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The account type that this GL/Project represents
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Start date
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// List of poolee accounts in a year.
        /// </summary>
        public IDictionary<string, string> PooleeAccounts;

        public List<string> GrantIds = new List<string>();        
    }
}
