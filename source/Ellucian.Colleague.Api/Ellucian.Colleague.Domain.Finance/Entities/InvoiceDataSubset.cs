// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Bitwise enumeration used when limiting file reads in the AccountsReceivableRepository
    /// </summary>
    [Serializable]
    [Flags]
    public enum InvoiceDataSubset { InvoiceOnly = 0, InvoicePayment = 1};   
}
