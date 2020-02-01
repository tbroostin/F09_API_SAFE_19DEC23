// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentQuickRegistrationEntityToStudentQuickRegistrationDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.StudentQuickRegistration, Ellucian.Colleague.Dtos.Student.QuickRegistration.StudentQuickRegistration>
    {
        public StudentQuickRegistrationEntityToStudentQuickRegistrationDtoAdapter(IAdapterRegistry adapterRegistry, slf4net.ILogger logger) 
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.QuickRegistrationTerm, Ellucian.Colleague.Dtos.Student.QuickRegistration.QuickRegistrationTerm>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.QuickRegistration.QuickRegistrationSection, Ellucian.Colleague.Dtos.Student.QuickRegistration.QuickRegistrationSection>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.DegreePlans.WaitlistStatus, Ellucian.Colleague.Dtos.Student.DegreePlans.WaitlistStatus>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.GradingType, Ellucian.Colleague.Dtos.Student.GradingType>();
        }
    }
}
