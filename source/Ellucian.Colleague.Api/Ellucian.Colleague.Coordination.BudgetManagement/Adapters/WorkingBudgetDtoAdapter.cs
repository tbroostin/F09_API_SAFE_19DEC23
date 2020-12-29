// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for working budget entity to Dto mapping.
    /// </summary>
    public class WorkingBudgetDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkingBudgetDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public WorkingBudgetDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>();
        }
    }
}