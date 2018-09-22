// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using System;


namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps a SectionAttendance Entity to a SectionAttendance Dto.
    /// </summary>
    public class SectionAttendanceResponseEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionAttendanceResponse, Ellucian.Colleague.Dtos.Student.SectionAttendanceResponse>
    {
        /// <summary>
        /// Initializes a new instance of the SectionAttendance Entity to Dto Adapter class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SectionAttendanceResponseEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentAttendance, Ellucian.Colleague.Dtos.Student.StudentAttendance>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionMeetingInstance, Ellucian.Colleague.Dtos.Student.SectionMeetingInstance>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.StudentSectionAttendanceError, Ellucian.Colleague.Dtos.Student.StudentSectionAttendanceError>();
        }
    }
}
