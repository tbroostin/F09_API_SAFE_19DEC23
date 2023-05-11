// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.Requirements;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentProgramEntityToApplicantStudentProgramDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, ApplicantStudentProgram>
    {
        public StudentProgramEntityToApplicantStudentProgramDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requirements.AdditionalRequirement, AdditionalRequirement2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentMajors, StudentMajors>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentMinors, StudentMinors>();
        }
    }
}
