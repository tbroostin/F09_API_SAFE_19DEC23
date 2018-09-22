// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping Section entity to Section3 DTO
    /// </summary>
    public class SectionEntityToSection3DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section3>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionEntityToSection3DtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public SectionEntityToSection3DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionBook, Ellucian.Colleague.Dtos.Student.SectionBook>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requisite, Ellucian.Colleague.Dtos.Student.Requisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Ellucian.Colleague.Dtos.Student.SectionRequisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionCharge, Ellucian.Colleague.Dtos.Student.SectionCharge>();
        }
    }
}