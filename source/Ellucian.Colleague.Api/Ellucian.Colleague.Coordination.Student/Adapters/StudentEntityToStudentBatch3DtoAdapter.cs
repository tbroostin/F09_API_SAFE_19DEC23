// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentEntityToStudentBatch3DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.StudentBatch3>
    {
        public StudentEntityToStudentBatch3DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.HighSchoolGpa, HighSchoolGpa>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Advisement, Advisement>();
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.Address, Dtos.Base.Address>();
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber, Dtos.Base.PhoneNumber>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentAcademicLevel, StudentAcademicLevel>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentHomeLocation, StudentHomeLocation>();
        }
    }
}