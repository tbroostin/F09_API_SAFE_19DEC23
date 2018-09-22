// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// An AR Deposit Type
    /// </summary>
    [Serializable]
    public class DepositType : CodeItem
    {
        /// <summary>
        /// Constructor for a deposit type
        /// </summary>
        /// <param name="code">Deposit type code</param>
        /// <param name="description">Deposit type description</param>
        public DepositType(string code, string description)
            : base(code, description)
        {
        }
    }
}
