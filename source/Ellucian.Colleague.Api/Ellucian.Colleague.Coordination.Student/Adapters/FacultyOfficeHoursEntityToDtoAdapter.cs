// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps an FacultyOfficeHours entity to an FacultyOfficeHours DTO
    /// </summary>
    public class FacultyOfficeHoursEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.FacultyOfficeHours, Ellucian.Colleague.Dtos.Student.FacultyOfficeHours>
    {
        /// <summary>
        /// FacultyOfficeHours entity adapter (current Entity to FacultyOfficeHours DTO) constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public FacultyOfficeHoursEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Student.Entities.FacultyOfficeHours, Dtos.Student.FacultyOfficeHours>();
            AddMappingDependency<Domain.Student.Entities.OfficeHours, Dtos.Student.OfficeHours>();
        }
    }
}

