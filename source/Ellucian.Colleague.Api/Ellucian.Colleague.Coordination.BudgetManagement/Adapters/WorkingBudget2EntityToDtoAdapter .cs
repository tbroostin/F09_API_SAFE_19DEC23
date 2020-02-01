// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for the workingbudget2 entity to Dto mapping.
    /// </summary>
    public class WorkingBudget2EntityToDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.WorkingBudget2, Dtos.BudgetManagement.WorkingBudget2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkingBudget2EntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public WorkingBudget2EntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.BudgetManagement.Entities.LineItem, Dtos.BudgetManagement.LineItem>();
        }
    }
}