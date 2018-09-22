// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping the GL object code entity to a DTO. 
    /// </summary>
    public class GlObjectCodeEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlObjectCode, Ellucian.Colleague.Dtos.ColleagueFinance.GlObjectCode>
    {
        public GlObjectCodeEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a Gl Object Code domain entity and all of its child objects into DTOs.
        /// </summary>
        /// <param name="Source">Gl Object Code domain entity to be converted.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions to format GL accounts.</param>
        /// <returns>Gl Object Code DTO.</returns>
        public GlObjectCode MapToType(Domain.ColleagueFinance.Entities.GlObjectCode Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glObjectCodeDto = new Dtos.ColleagueFinance.GlObjectCode();
            glObjectCodeDto.Id = Source.Id;
            glObjectCodeDto.Name = Source.Name;
            glObjectCodeDto.TotalBudget = Source.TotalBudget;
            glObjectCodeDto.TotalEncumbrances = Source.TotalEncumbrances;
            glObjectCodeDto.TotalActuals = Source.TotalActuals;

            // Translate the domain GlClass into the DTO GlClass
            switch (Source.GlClass)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Asset:
                    glObjectCodeDto.GlClass = Dtos.ColleagueFinance.GlClass.Asset;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Expense:
                    glObjectCodeDto.GlClass = Dtos.ColleagueFinance.GlClass.Expense;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.FundBalance:
                    glObjectCodeDto.GlClass = Dtos.ColleagueFinance.GlClass.FundBalance;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Liability:
                    glObjectCodeDto.GlClass = Dtos.ColleagueFinance.GlClass.Liability;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlClass.Revenue:
                    glObjectCodeDto.GlClass = Dtos.ColleagueFinance.GlClass.Revenue;
                    break;
            }

            glObjectCodeDto.GlAccounts = new List<Dtos.ColleagueFinance.GlObjectCodeGlAccount>();

            // Initialize the adapter to convert the GL accounts within the GL object code.
            var glAccountDtoAdapter = new GlObjectCodeGlAccountEntityToDtoAdapter(adapterRegistry, logger);

            // Convert the GL account domain entities into DTOs.
            foreach (var glAccount in Source.GlAccounts)
            {
                if (glAccount != null)
                {
                    // Add the GL account DTO to the GL object code DTO.
                    glObjectCodeDto.GlAccounts.Add(glAccountDtoAdapter.MapToType(glAccount, glMajorComponentStartPositions));
                }
            }

            // Initialize the adapter to convert the GL budget pools within the GL object code.
            var budgetPoolAdapter = new GlObjectCodeGlBudgetPoolEntityToDtoAdapter(adapterRegistry, logger);

            // Convert the pool domain entities into DTOs.
            glObjectCodeDto.Pools = new List<GlObjectCodeBudgetPool>();
            foreach (var pool in Source.Pools)
            {
                if (pool != null)
                {
                    glObjectCodeDto.Pools.Add(budgetPoolAdapter.MapToType(pool, glMajorComponentStartPositions));
                }
            }

            return glObjectCodeDto;
        }

    }
}
