using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.Planning.Tests
{
    public class TestPlanningConfigurationRepository : IPlanningConfigurationRepository
    {
        public async Task<PlanningConfiguration> GetPlanningConfigurationAsync()
        {
            return new PlanningConfiguration()
            {
                DefaultCatalogPolicy = CatalogPolicy.CurrentCatalogYear,
                DefaultCurriculumTrack = null
            };
        }
    }
}
