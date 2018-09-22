// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentWaiverEntityToDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver> {
        public StudentWaiverEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.RequisiteWaiver, Dtos.Student.RequisiteWaiver>();
        }
    }
}
