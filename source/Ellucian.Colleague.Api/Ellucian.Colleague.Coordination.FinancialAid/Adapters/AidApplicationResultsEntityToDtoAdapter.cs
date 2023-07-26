// Copyright 2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    public class AidApplicationResultsEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AidApplicationResults, Dtos.FinancialAid.AidApplicationResults>
    {
        public AidApplicationResultsEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AssumedStudentDetails, Dtos.FinancialAid.AssumedStudentDetails>();
            AddMappingDependency<Domain.FinancialAid.Entities.AlternatePrimaryEfc, Dtos.FinancialAid.AlternatePrimaryEfc>();
        }
    }
}
