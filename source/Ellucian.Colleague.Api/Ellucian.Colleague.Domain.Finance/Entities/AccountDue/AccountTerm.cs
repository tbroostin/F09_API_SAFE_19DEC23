// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountDue
{
    [Serializable]
    public class AccountTerm
    {
        public AccountTerm()
        {
            AccountDetails = new List<AccountsReceivableDueItem>();
        }

        public decimal Amount { get; set; }

        public string TermId { get; set; }

        public string Description { get; set; }

        public List<AccountsReceivableDueItem> AccountDetails { get; set; }

    }
}
