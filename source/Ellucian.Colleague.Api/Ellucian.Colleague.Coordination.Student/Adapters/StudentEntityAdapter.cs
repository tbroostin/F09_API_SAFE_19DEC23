// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>
    {
        public StudentEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.HighSchoolGpa, HighSchoolGpa>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Advisement, Advisement>();
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentHomeLocation, Ellucian.Colleague.Dtos.Student.StudentHomeLocation>();
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.PersonHierarchyName, Ellucian.Colleague.Dtos.Base.PersonHierarchyName>();
        }
    }
}
