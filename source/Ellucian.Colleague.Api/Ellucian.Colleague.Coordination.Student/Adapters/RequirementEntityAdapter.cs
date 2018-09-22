// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    // Complex objects require additional dependency mappings
    public class RequirementEntityAdapter : AutoMapperAdapter<Domain.Student.Entities.Requirements.Requirement, Ellucian.Colleague.Dtos.Student.Requirements.Requirement>
    {
        public RequirementEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requirements.Group, Ellucian.Colleague.Dtos.Student.Requirements.Group>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requirements.Subrequirement, Ellucian.Colleague.Dtos.Student.Requirements.Subrequirement>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requirements.MaxCoursesAtLevels, Ellucian.Colleague.Dtos.Student.Requirements.MaxCoursesAtLevels>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requirements.MaxCreditAtLevels, Ellucian.Colleague.Dtos.Student.Requirements.MaxCreditAtLevels>();
        }
    }
}
