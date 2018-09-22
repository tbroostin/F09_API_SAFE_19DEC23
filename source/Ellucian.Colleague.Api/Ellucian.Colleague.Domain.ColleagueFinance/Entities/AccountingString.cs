// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Accounting String is used to search for an account number + project number and by valid on date
    /// </summary>
    [Serializable]
    public class AccountingString
    {
        /// <summary>
        /// AccountString
        /// </summary>
        public string AccountString { get; private set; }

        /// <summary>
        /// Project Number
        /// </summary>
        public string ProjectNumber { get; private set; }

        /// <summary>
        /// date to check if account string is valid on
        /// </summary>
        public DateTime? ValidOn { get; set; }

        /// <summary>
        /// Description for AccountingString
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="accountString"></param>
        /// <param name="projectNumber"></param>
        /// <param name="validOn"></param>
        public AccountingString(string accountString, string projectNumber = "", DateTime? validOn = null, string description = "")
        {
            if (string.IsNullOrEmpty(accountString))
            {
                throw new ArgumentNullException("accountString", "Accouting String is a required field.");
            }
            AccountString = accountString;
            ProjectNumber = projectNumber;
            ValidOn = validOn;
            Description = description;
        }
    }
}
