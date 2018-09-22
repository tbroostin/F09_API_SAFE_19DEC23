// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An invoice type
    /// </summary>
    [Serializable]
    public class InvoiceType : CodeItem
    {
        public InvoiceType(string code, string description)
            : base(code, description)
        {
        }
    }
}
