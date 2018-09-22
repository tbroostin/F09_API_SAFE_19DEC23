// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountDue
{
    [Serializable]
    public class AccountDuePeriod
    {
        public AccountDue Past { get; set; }

        public AccountDue Current { get; set; }

        public AccountDue Future { get; set; }

        public string PersonName { get; set; }
    }
}
