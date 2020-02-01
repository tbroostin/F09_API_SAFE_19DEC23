// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for a budget officer entity to Dto mapping.
    /// </summary>
    public class BudgetOfficerEntityToDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetOfficerEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public BudgetOfficerEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override BudgetOfficer MapToType(Domain.BudgetManagement.Entities.BudgetOfficer source)
        {
            var budgetOfficerDto = new BudgetOfficer();
            budgetOfficerDto.BudgetOfficerId = source.BudgetOfficerId;
            budgetOfficerDto.BudgetOfficerName = source.BudgetOfficerName;

            return budgetOfficerDto;
        }
    }
}