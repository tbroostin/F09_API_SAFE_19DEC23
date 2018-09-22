// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping the cost center entity to a DTO. 
    /// </summary>
    public class CostCenterEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CostCenter, Ellucian.Colleague.Dtos.ColleagueFinance.CostCenter>
    {
        public CostCenterEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a cost center domain entity and all of its subtotal objects into DTOs.
        /// </summary>
        /// <param name="Source">Cost center domain entity to be converted.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions to format GL accounts.</param>
        /// <returns>Cost center DTO.</returns>
        public CostCenter MapToType(Domain.ColleagueFinance.Entities.CostCenter Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var costCenterDto = new Dtos.ColleagueFinance.CostCenter();
            costCenterDto.Id = Source.Id;
            costCenterDto.Name = Source.Name;
            costCenterDto.UnitId = Source.UnitId;
            costCenterDto.TotalBudget = Source.TotalBudgetExpenses;
            costCenterDto.TotalEncumbrances = Source.TotalEncumbrancesExpenses;
            costCenterDto.TotalActuals = Source.TotalActualsExpenses;
            costCenterDto.TotalBudgetRevenue = Source.TotalBudgetRevenue;
            costCenterDto.TotalActualsRevenue = Source.TotalActualsRevenue;
            costCenterDto.TotalEncumbrancesRevenue = Source.TotalEncumbrancesRevenue;

            costCenterDto.CostCenterSubtotals = new List<Dtos.ColleagueFinance.CostCenterSubtotal>();

            // Initialize the adapter to convert the GL accounts within the cost center.
            var costCenterSubtotalDtoAdapter = new CostCenterSubtotalEntityToDtoAdapter(adapterRegistry, logger);

            // Convert the GL account domain entities into DTOs.
            foreach (var subtotal in Source.CostCenterSubtotals)
            {
                // Add the cost center subtotal DTO to the cost center DTO.
                var costCenterSubtotalDto = costCenterSubtotalDtoAdapter.MapToType(subtotal, glMajorComponentStartPositions);
                costCenterDto.CostCenterSubtotals.Add(costCenterSubtotalDto);
            }

            return costCenterDto;
        }

    }
}
