// Copyright 2017 Ellucian Company L.P. and its affiliates.using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a general ledger account.
    /// </summary>
    [Serializable]
    public class GeneralLedgerAccount
    {
        /// <summary>
        /// The cost center ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The part of the ID that corresponds to the unit component.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Formatted general ledger account number for display.
        /// </summary>
        public string FormattedGlAccount
        {
            get
            {
                // If the general ledger account is longer than 15 digits, it contains underscores
                // and we can convert these to hyphens.
                // Otherwise, obtain the delimiter positions from the general ledger account parameters
                // record and use them to insert the hyphens in the correct positions.
                string formattedGlAccount = string.Empty;
                if (Id.Length > 15)
                {
                    formattedGlAccount = Id.Replace("_", "-");
                }
                else
                {
                    formattedGlAccount = Id;
                    if (majorComponentStartPositions != null)
                    {
                        List<string> reversedStartPosition = new List<string>();
                        reversedStartPosition = majorComponentStartPositions.ToList();
                        reversedStartPosition.Reverse();
                        List<int> position = reversedStartPosition.ConvertAll(s => Int32.Parse(s));
                        var tempFormattedGlAccount = new StringBuilder(Id);
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
                    
                }
                return formattedGlAccount;
            }
        }
        private IEnumerable<string> majorComponentStartPositions;

        /// <summary>
        /// Initializes the General ledger account object.
        /// </summary>
        public GeneralLedgerAccount(string id, IEnumerable<string> majorComponentStartPositionsIn)
        {
            this.id = id;
            this.majorComponentStartPositions = majorComponentStartPositionsIn;
        }
    }
}