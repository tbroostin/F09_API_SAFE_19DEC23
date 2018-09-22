using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Utility.Adapters;
using Ellucian.Utility.Logging;
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    // Complex objects require additional dependency mappings
    public class CourseEntityAdapter : AutoMapperAdapter<Course, Ellucian.Colleague.Dtos.Student.Course>
    {
        public CourseEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Corequisite, Ellucian.Colleague.Dtos.Student.Corequisite>();
        }
    }
}
