/* Copyright 2016-2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    public class PersonPositionEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.PersonPosition, Dtos.HumanResources.PersonPosition>
    {
        public PersonPositionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.WorkScheduleItem, Dtos.HumanResources.WorkScheduleItem>();
        }
    }
}
