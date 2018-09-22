//Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom adapter to convert AwardLetterConfiguration entity to AwardLetterConfiguration DTO
    /// </summary>
    public class AwardLetterConfigurationEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AwardLetterConfiguration, Dtos.FinancialAid.AwardLetterConfiguration>
    {

        public AwardLetterConfigurationEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AwardLetterGroup2, Dtos.FinancialAid.AwardLetterGroup>();            
        }
    
    }
}
