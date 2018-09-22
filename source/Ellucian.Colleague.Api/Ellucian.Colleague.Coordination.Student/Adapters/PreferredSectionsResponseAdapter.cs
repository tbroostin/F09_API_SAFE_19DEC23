// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class PreferredSectionsResponseAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionsResponse, Ellucian.Colleague.Dtos.Student.PreferredSectionsResponse>
    {
        public PreferredSectionsResponseAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.PreferredSection, Ellucian.Colleague.Dtos.Student.PreferredSection>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.PreferredSectionMessage, Ellucian.Colleague.Dtos.Student.PreferredSectionMessage>();
        }
    }
}
