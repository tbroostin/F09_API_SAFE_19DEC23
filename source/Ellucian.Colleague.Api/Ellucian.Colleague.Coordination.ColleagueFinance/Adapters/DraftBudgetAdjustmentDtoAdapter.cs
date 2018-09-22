// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{

    /// <summary>
    /// Adapter for a Draft Budget Adjustment entity to Dto mapping.
    /// </summary>
    public class DraftBudgetAdjustmentDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.DraftBudgetAdjustment, Dtos.ColleagueFinance.DraftBudgetAdjustment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DraftBudgetAdjustmentDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public DraftBudgetAdjustmentDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.DraftAdjustmentLine, Dtos.ColleagueFinance.DraftAdjustmentLine>();

            AddMappingDependency<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>();
        }
    }
}
