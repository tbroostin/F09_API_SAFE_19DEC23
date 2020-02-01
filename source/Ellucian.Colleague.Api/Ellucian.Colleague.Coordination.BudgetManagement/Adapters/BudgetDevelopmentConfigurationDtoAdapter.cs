// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for Budget Configuration entity to Dto mapping.
    /// </summary>
    public class BudgetConfigurationDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetConfiguration, Dtos.BudgetManagement.BudgetConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfigurationDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public BudgetConfigurationDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetConfigurationComparable, Dtos.BudgetManagement.BudgetConfigurationComparable>();
        }
    }
}
