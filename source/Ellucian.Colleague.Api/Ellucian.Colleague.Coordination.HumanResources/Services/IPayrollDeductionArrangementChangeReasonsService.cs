﻿//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPayrollDeductionArrangementChangeReasonsService
    {
        Task<IEnumerable<Dtos.PayrollDeductionArrangementChangeReason>> GetPayrollDeductionArrangementChangeReasonsAsync(bool bypassCache = false);
        Task<Dtos.PayrollDeductionArrangementChangeReason> GetPayrollDeductionArrangementChangeReasonByIdAsync(string id);
    }
}
