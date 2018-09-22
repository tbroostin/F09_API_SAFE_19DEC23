// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;
using System.Linq;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping the cost center entity to a DTO. 
    /// </summary>
    public class CostCenterSubtotalEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CostCenterSubtotal, Ellucian.Colleague.Dtos.ColleagueFinance.CostCenter>
    {
        public CostCenterSubtotalEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a cost center subtotal domain entity and all of its GL account objects into DTOs.
        /// </summary>
        /// <param name="Source">Cost center domain entity to be converted.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions to format GL accounts.</param>
        /// <returns>Cost center DTO.</returns>
        public CostCenterSubtotal MapToType(Domain.ColleagueFinance.Entities.CostCenterSubtotal Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var costCenterSubtotalDto = new Dtos.ColleagueFinance.CostCenterSubtotal();
            costCenterSubtotalDto.Id = Source.Id;
            costCenterSubtotalDto.Name = Source.Name;
            costCenterSubtotalDto.IsDefined = Source.IsDefined;

            // Translate the domain GlClass into the DTO GlClass
            switch (Source.GlClass)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Asset:
                    costCenterSubtotalDto.GlClass = Dtos.ColleagueFinance.GlClass.Asset;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Expense:
                    costCenterSubtotalDto.GlClass = Dtos.ColleagueFinance.GlClass.Expense;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.FundBalance:
                    costCenterSubtotalDto.GlClass = Dtos.ColleagueFinance.GlClass.FundBalance;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Liability:
                    costCenterSubtotalDto.GlClass = Dtos.ColleagueFinance.GlClass.Liability;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Revenue:
                    costCenterSubtotalDto.GlClass = Dtos.ColleagueFinance.GlClass.Revenue;
                    break;
            }

            costCenterSubtotalDto.TotalBudget = Source.TotalBudget;
            costCenterSubtotalDto.TotalEncumbrances = Source.TotalEncumbrances;
            costCenterSubtotalDto.TotalActuals = Source.TotalActuals;

            costCenterSubtotalDto.GlAccounts = new List<Dtos.ColleagueFinance.CostCenterGlAccount>();

            // Initialize the adapter to convert the GL accounts within the cost center subtotal.
            var glAccountDtoAdapter = new GlAccountEntityToDtoAdapter(adapterRegistry, logger);

            // Convert the GL account domain entities into DTOs.
            foreach (var glAccount in Source.GlAccounts)
            {
                // Add the GL account DTO to the cost center subtotal DTO.
                var glAccountDto = glAccountDtoAdapter.MapToType(glAccount, glMajorComponentStartPositions);
                costCenterSubtotalDto.GlAccounts.Add(glAccountDto);
            }

            // Convert the pool domain entities into DTOs.
            var budgetPoolAdapter = new GlBudgetPoolEntityToDtoAdapter(adapterRegistry, logger);
            costCenterSubtotalDto.Pools = Source.Pools.Select(x => budgetPoolAdapter.MapToType(x, glMajorComponentStartPositions)).ToList();

            return costCenterSubtotalDto;
        }

    }
}
