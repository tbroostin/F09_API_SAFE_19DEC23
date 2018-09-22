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
    /// Maps SectionMeeting entity to SectionMeeting (version 1) DTO
    /// </summary>
    public class SectionMeetingEntityToSectionMeetingDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SectionMeetingEntityToSectionMeetingDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public SectionMeetingEntityToSectionMeetingDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Maps the SectionMeeting entity to SectionMeeting (version 1) DTO.
        /// </summary>
        /// <param name="Source">The SectionMeeting entity.</param>
        /// <returns>SectionMeeting DTO</returns>
        public override Dtos.Student.SectionMeeting MapToType(Domain.Student.Entities.SectionMeeting Source)
        {
            var sectionMeetingDto = base.MapToType(Source);
            // The only fields that can't be automapped are the StartTime and EndTime fields
            // which are of type TimeStamp. (SectionMeeting entity now have these as DateTimeOffset)
            sectionMeetingDto.StartTime = Source.StartTime.HasValue ?
                Source.StartTime.Value.DateTime.TimeOfDay.ToString() : string.Empty;
            sectionMeetingDto.EndTime = Source.EndTime.HasValue ?
                Source.EndTime.Value.DateTime.TimeOfDay.ToString() : string.Empty;

            return sectionMeetingDto;
        }
    }
}