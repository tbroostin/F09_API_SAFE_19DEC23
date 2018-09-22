// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public class ActivityDateTermItem : ActivityTermItem
    {
        public DateTime? Date { get; set; }
    }
}
