// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class ProgramRequirementsDtoAdapter : AutoMapperAdapter<Domain.Student.Entities.Requirements.ProgramRequirements, Ellucian.Colleague.Dtos.Student.Requirements.ProgramRequirements>
    {
        public ProgramRequirementsDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.Requirements.Requirement, Ellucian.Colleague.Dtos.Student.Requirements.Requirement>();
            AddMappingDependency<Domain.Student.Entities.Requirements.Subrequirement, Ellucian.Colleague.Dtos.Student.Requirements.Subrequirement>();
            AddMappingDependency<Domain.Student.Entities.Requirements.Group, Ellucian.Colleague.Dtos.Student.Requirements.Group>();
        }
    }
}
