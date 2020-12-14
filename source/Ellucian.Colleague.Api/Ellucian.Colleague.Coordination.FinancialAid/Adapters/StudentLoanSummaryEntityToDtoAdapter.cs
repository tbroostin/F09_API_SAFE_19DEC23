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
    /// Maps a StudentLoanSummary Entity to the DTO
    /// </summary>
    public class StudentLoanSummaryEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.StudentLoanSummary, Dtos.FinancialAid.StudentLoanSummary>
    {
        public StudentLoanSummaryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.StudentLoanHistory, Dtos.FinancialAid.StudentLoanHistory>();
            AddMappingDependency<Domain.FinancialAid.Entities.InformedBorrowerItem, Dtos.FinancialAid.InformedBorrowerItem>();
        }
    }
}
