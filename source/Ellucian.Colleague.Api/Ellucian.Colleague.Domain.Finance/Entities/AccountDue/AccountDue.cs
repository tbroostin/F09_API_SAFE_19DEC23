// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountDue
{
    [Serializable]
    public class AccountDue
    {
        public AccountDue()
        {
            AccountTerms = new List<AccountTerm>();
        }

        public string PersonId { get; set; }

        public List<AccountTerm> AccountTerms { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string PersonName { get; set; }
    }
}
