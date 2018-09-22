// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
    public class AcademicHistoryEntityToAcademicHistory2DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicHistory, Ellucian.Colleague.Dtos.Student.AcademicHistory2>
    {
        public AcademicHistoryEntityToAcademicHistory2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.AcademicTerm, Ellucian.Colleague.Dtos.Student.AcademicTerm2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.AcademicCredit, Ellucian.Colleague.Dtos.Student.AcademicCredit2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.GradeRestriction, Ellucian.Colleague.Dtos.Student.GradeRestriction>();
        }
    }
}
