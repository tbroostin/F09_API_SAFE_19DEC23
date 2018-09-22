// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class TermEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>
    {
        public TermEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.RegistrationDate, RegistrationDate>();
        }
    }
}
