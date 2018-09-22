// Copyright 2016 Ellucian Company L.P. and its affiliates.
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
    /// <summary>
    /// Convert PilotGrade entity to PilotGrade DTO
    /// </summary>
    public class PilotGradeAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotGrade, Ellucian.Colleague.Dtos.Student.PilotGrade>
    {
        public PilotGradeAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }
    }
}