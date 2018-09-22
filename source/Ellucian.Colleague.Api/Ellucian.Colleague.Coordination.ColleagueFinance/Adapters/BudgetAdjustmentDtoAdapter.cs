// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{

    /// <summary>
    /// Adapter for Budget Adjustment entity to Dto mapping.
    /// </summary>
    public class BudgetAdjustmentDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetAdjustmentDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public BudgetAdjustmentDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.AdjustmentLine, Dtos.ColleagueFinance.AdjustmentLine>();

            AddMappingDependency<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>();

            AddMappingDependency<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>();
        }
    }
}
