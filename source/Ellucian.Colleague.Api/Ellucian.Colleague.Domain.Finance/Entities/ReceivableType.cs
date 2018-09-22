// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    [Serializable]
    public class ReceivableType : CodeItem
    {
        public ReceivableType(string code, string description)
            : base(code, description)
        {
        }
    }
}
