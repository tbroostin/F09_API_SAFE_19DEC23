// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
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
    // Complex objects require additional dependency mappings
    public class AcademicHistoryLevelEntityToAcademicHistoryLevel2DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.AcademicHistoryLevel2>
    {
        public AcademicHistoryLevelEntityToAcademicHistoryLevel2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistoryBatch2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm3>();
        }
    }
}
