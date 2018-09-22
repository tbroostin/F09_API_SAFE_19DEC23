//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Maps loan request Entity to DTO
    /// </summary>
    public class LoanRequestEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.LoanRequest, Dtos.FinancialAid.LoanRequest>
    {
        public LoanRequestEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.LoanRequestPeriod, Dtos.FinancialAid.LoanRequestPeriod>();
        }
    }
}
