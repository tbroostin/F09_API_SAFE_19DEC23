
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class GlBudgetPoolEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlBudgetPool, Ellucian.Colleague.Dtos.ColleagueFinance.GlBudgetPool>
    {
        public GlBudgetPoolEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a GL budget pool domain entity into a DTO.
        /// </summary>
        /// <param name="Source">A GL budget pool domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL account DTO.</returns>
        public Dtos.ColleagueFinance.GlBudgetPool MapToType(Domain.ColleagueFinance.Entities.GlBudgetPool Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glBudgetPoolDto = new GlBudgetPool();

            // Copy the GL budget pool properties.
            var glAccountDtoAdapter = new GlAccountEntityToDtoAdapter(adapterRegistry, logger);
            var glAccountDto = glAccountDtoAdapter.MapToType(Source.Umbrella, glMajorComponentStartPositions);

            // If the umbrella is not visible to the user, null out the amounts, so they will displayed masked in SS.
            if (!Source.IsUmbrellaVisible)
            {
                glAccountDto.Actuals = 0;
                glAccountDto.Budget = 0;
                glAccountDto.Encumbrances = 0;
            }

            glBudgetPoolDto.Umbrella = glAccountDto;
            glBudgetPoolDto.Poolees = Source.Poolees.Select(x => glAccountDtoAdapter.MapToType(x, glMajorComponentStartPositions)).ToList();
            glBudgetPoolDto.IsUmbrellaVisible = Source.IsUmbrellaVisible;

            return glBudgetPoolDto;
        }
    }
}
