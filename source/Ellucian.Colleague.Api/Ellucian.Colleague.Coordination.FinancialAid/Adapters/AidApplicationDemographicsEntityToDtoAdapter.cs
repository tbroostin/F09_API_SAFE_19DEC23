// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    public class AidApplicationDemographicsEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AidApplicationDemographics, Dtos.FinancialAid.AidApplicationDemographics>
    {
        public AidApplicationDemographicsEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.Address, Dtos.FinancialAid.Address>();
        }
    }
}
