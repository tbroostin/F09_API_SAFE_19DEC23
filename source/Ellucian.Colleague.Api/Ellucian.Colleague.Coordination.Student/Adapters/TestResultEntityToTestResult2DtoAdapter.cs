// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping from the TestResult Domain Entity to the TestResult2 DTO 
    /// </summary>
    public class TestResultEntityToTestResult2DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>
    {
        public TestResultEntityToTestResult2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependencies
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult2>();
        }
    }
}
