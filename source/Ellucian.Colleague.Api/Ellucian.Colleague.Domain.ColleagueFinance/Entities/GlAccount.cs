// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents the common components in a GL account.
    /// </summary>
    [Serializable]
    public class GlAccount
    {
        /// <summary>
        /// A GL account number.
        /// </summary>
        public string GlAccountNumber { get { return accountNumber; } }
        private readonly string accountNumber;

        /// <summary>
        /// Description for the GL account.
        /// </summary>
        public string GlAccountDescription { get; set; }

        /// <summary>
        /// Initialize the GL Account.
        /// </summary>
        /// <param name="glAccount"></param>
        public GlAccount(string glAccount)
        {
            if (string.IsNullOrEmpty(glAccount))
                throw new ArgumentNullException("glAccount", "GL Account is a required field.");

            this.accountNumber = glAccount;
        }

        /// <summary>
        /// Formatted general ledger account number for display.
        /// </summary>
        public string GetFormattedGlAccount(IEnumerable<string> majorComponentStartPosition)
        {
            // If the general ledger account is longer than 15 digits, it contains underscores
            // and we can convert these to hyphens.
            // Otherwise, obtain the delimiter positions from the general ledger account parameters
            // record and use them to insert the hyphens in the correct positions.
            string formattedGlAccount = string.Empty;
            if (accountNumber.Length > 15)
            {
                formattedGlAccount = accountNumber.Replace("_", "-");
            }
            else
            {
                List<string> reversedStartPosition = new List<string>();
                reversedStartPosition = majorComponentStartPosition.ToList();
                reversedStartPosition.Reverse();
                List<int> position = reversedStartPosition.ConvertAll(s => Int32.Parse(s));
                var tempFormattedGlAccount = new StringBuilder(accountNumber);
                foreach (int pos in position)
                {
                    // Only insert the hyphen if the position is a positive number
                    if (pos > 1)
                    {
                        // Because C# indexes begin at zero we subtract 1 from the position to insert the delimiter in the correct position.
                        tempFormattedGlAccount.Insert(pos - 1, '-');
                    }
                }
                formattedGlAccount = tempFormattedGlAccount.ToString();
            }
            return formattedGlAccount;
        }
    }
}