//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Payroll deduction arrangement change reason.
    /// </summary>
    [Serializable]
    public class PayrollDeductionArrangementChangeReason: GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayrollDeductionArrangementChangeReason"/> class.
        /// </summary>
        public PayrollDeductionArrangementChangeReason(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
