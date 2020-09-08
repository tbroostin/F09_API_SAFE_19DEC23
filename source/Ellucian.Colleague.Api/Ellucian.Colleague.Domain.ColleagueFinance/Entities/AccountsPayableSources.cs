// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Class for Accounts Payable Sources
    /// </summary>
    [Serializable]
    public class AccountsPayableSources : GuidCodeItem
    {

        /// <summary>
        /// The direct deposit status for the account payable source
        /// </summary>
        public string directDeposit { get; set; }

        /// <summary>
        /// The  Source of the account payable source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// AP Accounts Payable Sources
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public AccountsPayableSources(string guid, string code, string description)
            : base(guid, code, description)
        {
            // no additional work to do
        }
    }
}

