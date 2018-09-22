// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Finance.Entities.AccountActivity
{
    [Serializable]
    public partial class FeeType : NamedType
    {
        public List<ActivityDateTermItem> FeeCharges { get; set; }
    }
}