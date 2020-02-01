// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    /// <summary>
    /// finance query gl account line item entity which contains gl account details including budget pools
    /// </summary>
    [Serializable]
    public class FinanceQueryGlAccountLineItem
    {
        /// <summary>
        /// gl account number, used as identifier
        /// </summary>
        public string GlAccountNumber { get; set; }
        
        /// <summary>
        /// primary gl account.
        /// </summary>
        public FinanceQueryGlAccount GlAccount { get; set; }

        /// <summary>
        /// Whether the umbrella GL account would be visible to the user
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaVisible { get; set; }

        /// <summary>
        /// whether gl account is umbrella or non-pooled account
        /// given their GL account security setup.
        /// </summary>
        public bool IsUmbrellaAccount { get; set; }

        /// <summary>
        /// string containing gl component values used for sorting,separated by "-"
        /// </summary>
        public string SortKey { get; set; }


        /// <summary>
        /// List of GL Budget pools included in the Umbrella GlAccount.
        /// </summary>
        public List<FinanceQueryGlAccount> Poolees = new List<FinanceQueryGlAccount>();

        /// <summary>
        /// Constructor that initializes a finance query gl account line item object.
        /// </summary>
        /// <param name="glAccount">gl account.</param>
        /// <param name="isPooledAccount">whether gl account is non-pooled or budget pool</param>
        /// <param name="isUmbrellaVisible">whether umbrella account has been given access to the user</param>
        public FinanceQueryGlAccountLineItem(FinanceQueryGlAccount glAccount, bool isPooledAccount, bool isUmbrellaVisible)
        {
            if (glAccount == null)
            {
                throw new ArgumentNullException("glAccount","GL Account is a required field.");
            }

            this.GlAccountNumber = glAccount.GlAccountNumber;
            this.GlAccount = glAccount;
            
            this.IsUmbrellaAccount = isPooledAccount;
            this.IsUmbrellaVisible = isUmbrellaVisible;
        }

    }
}