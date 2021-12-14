// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountDue
{
    [Serializable]
    public class AccountsReceivableDueItem
    {
        public decimal? AmountDue { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTimeOffset? DueDateOffset { get; set; }

        public string Description { get; set; }

        public bool Overdue { get; set; }

        public string Term { get; set; }

        public string TermDescription { get; set; }

        public string Period { get; set; }

        public string PeriodDescription { get; set; }

        public string AccountType { get; set; }

        public string AccountDescription { get; set; }

        public string Distribution { get; set; }
    }
}
