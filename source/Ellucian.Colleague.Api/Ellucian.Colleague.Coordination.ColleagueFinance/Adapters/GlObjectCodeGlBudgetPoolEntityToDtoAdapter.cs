// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class GlObjectCodeGlBudgetPoolEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPool, Ellucian.Colleague.Dtos.ColleagueFinance.GlBudgetPool>
    {
        public GlObjectCodeGlBudgetPoolEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a GL budget pool domain entity into a DTO.
        /// </summary>
        /// <param name="Source">A GL budget pool domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL budget pool DTO.</returns>
        public Dtos.ColleagueFinance.GlObjectCodeBudgetPool MapToType(Domain.ColleagueFinance.Entities.GlObjectCodeBudgetPool Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glBudgetPoolDto = new GlObjectCodeBudgetPool();

            // Copy the GL budget pool properties.
            var glAccountDtoAdapter = new GlObjectCodeGlAccountEntityToDtoAdapter(adapterRegistry, logger);
            var glAccountDto = glAccountDtoAdapter.MapToType(Source.Umbrella, glMajorComponentStartPositions);

            // If the umbrella is not visible to the user, null out the amounts, so they will displayed masked in SS.
            if (!Source.IsUmbrellaVisible)
            {
                glAccountDto.Actuals = 0;
                glAccountDto.Budget = 0;
                glAccountDto.Encumbrances = 0;
            }

            glBudgetPoolDto.Umbrella = glAccountDto;
            glBudgetPoolDto.IsUmbrellaVisible = Source.IsUmbrellaVisible;
            glBudgetPoolDto.Poolees = new List<Dtos.ColleagueFinance.GlObjectCodeGlAccount>();
            foreach (var pooleeEntity in Source.Poolees)
            {
                if (pooleeEntity != null)
                {
                    glBudgetPoolDto.Poolees.Add(glAccountDtoAdapter.MapToType(pooleeEntity, glMajorComponentStartPositions));
                }
            }

            return glBudgetPoolDto;
        }
    }
}
