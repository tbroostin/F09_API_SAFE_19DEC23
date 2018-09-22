// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountDue
{
    [Serializable]
    public class InvoiceDueItem : AccountsReceivableDueItem
    {
        public string InvoiceId { get; set; }
    }
}
