// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping Section entity to Section4 DTO
    /// </summary>
    public class SectionEntityToStudentSection4DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionEntityToStudentSection4DtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public SectionEntityToStudentSection4DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionBook, Ellucian.Colleague.Dtos.Student.SectionBook>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requisite, Ellucian.Colleague.Dtos.Student.Requisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Ellucian.Colleague.Dtos.Student.SectionRequisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionCharge, Ellucian.Colleague.Dtos.Student.SectionCharge>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionCensusCertification, Ellucian.Colleague.Dtos.Student.SectionCensusCertification>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionFaculty, Ellucian.Colleague.Dtos.Student.SectionFaculty>();
        }
    }
}