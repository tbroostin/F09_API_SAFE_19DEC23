// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Web.Adapters;
using slf4net;


namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping Section entity to Section3 DTO
    /// </summary>
    public class SectionEntityToSection4DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Section4>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionEntityToSection3DtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public SectionEntityToSection4DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting2>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionBook, Ellucian.Colleague.Dtos.Student.SectionBook>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requisite, Ellucian.Colleague.Dtos.Student.Requisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Ellucian.Colleague.Dtos.Student.SectionRequisite>();
        }
    }
}