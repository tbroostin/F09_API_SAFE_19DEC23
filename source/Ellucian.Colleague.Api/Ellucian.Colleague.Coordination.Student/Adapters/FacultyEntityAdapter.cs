// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters 
{
    public class FacultyEntityAdapter : AutoMapperAdapter<Domain.Student.Entities.Faculty, Dtos.Student.Faculty> {
        public FacultyEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) {
            // Mapping dependency
            AddMappingDependency<Domain.Base.Entities.Phone, Dtos.Base.Phone>();
        }
    }
}
