// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping the finance query summary code entity to a DTO. 
    /// </summary>
    public class FinanceQueryEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinanceQuery, Ellucian.Colleague.Dtos.ColleagueFinance.FinanceQuery>
    {
        public FinanceQueryEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a Finance query summary domain entity and all of its child objects into DTOs.
        /// </summary>
        /// <param name="Source">Gl Object Code domain entity to be converted.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions to format GL accounts.</param>
        /// <returns>Gl Object Code DTO.</returns>
        public FinanceQuery MapToType(Domain.ColleagueFinance.Entities.FinanceQuery Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var financeQueryDto = new Dtos.ColleagueFinance.FinanceQuery();
           
            financeQueryDto.TotalBudget = Source.TotalBudget;
            financeQueryDto.TotalEncumbrances = Source.TotalEncumbrances;
            financeQueryDto.TotalActuals = Source.TotalActuals;
            financeQueryDto.TotalRequisitions = Source.TotalRequisitions;
            financeQueryDto.SubTotals = new List<FinanceQuerySubtotal>();


            var subtotalsAdapter = new FinanceQuerySubtotalEntityToDtoAdapter(adapterRegistry, logger);
            foreach (var subTotal in Source.FinanceQuerySubtotals)
            {
                var financeQuerySubTotal = subtotalsAdapter.MapTotype(subTotal, glMajorComponentStartPositions);
                financeQueryDto.SubTotals.Add(financeQuerySubTotal);
            }
            
            return financeQueryDto;
        }

    }
}
