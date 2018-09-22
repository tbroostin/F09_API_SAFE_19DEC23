// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping from the TestScore DTO to the TestScore domain entity.
    /// </summary>
    public class TestScoreAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.TestScore, TestScore>
    {
        public TestScoreAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Dtos.Student.CustomField, CustomField>();
        }
    }
}
