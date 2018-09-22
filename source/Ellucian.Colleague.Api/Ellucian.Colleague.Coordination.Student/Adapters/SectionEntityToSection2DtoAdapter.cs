// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    // Complex objects require additional dependency mappings
    public class SectionEntityToSection2DtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section2>
    {
        public SectionEntityToSection2DtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency 
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionBook, Ellucian.Colleague.Dtos.Student.SectionBook>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.Requisite, Ellucian.Colleague.Dtos.Student.Requisite>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite, Ellucian.Colleague.Dtos.Student.SectionRequisite>();
        }

        /// <summary>
        /// Custom mapping of Section entity to Section2 DTO.
        /// </summary>
        /// <param name="Source">The source.</param>
        /// <returns></returns>
        public override Dtos.Student.Section2 MapToType(Domain.Student.Entities.Section Source)
        {
            var section2Dto = base.MapToType(Source);
            // SectionMeeting v1 mapping needs to be handled differently since SectionMeeting2 was created.
            var meetingAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting>();
            var customMappedSectionMeetings = new List<Ellucian.Colleague.Dtos.Student.SectionMeeting>();
            foreach (var secMeeting in Source.Meetings)
            {
                customMappedSectionMeetings.Add(meetingAdapter.MapToType(secMeeting));
            }
            section2Dto.Meetings = customMappedSectionMeetings;
            return section2Dto;
        }
    }
}