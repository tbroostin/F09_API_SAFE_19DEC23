// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Class for Accounts Payable Tax codes.
    /// </summary>
    [Serializable]
    public class AccountsPayableTax : CodeItem
    {
        /// <summary>
        /// Accounts Payable Tax codes constructor.
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public AccountsPayableTax(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}


