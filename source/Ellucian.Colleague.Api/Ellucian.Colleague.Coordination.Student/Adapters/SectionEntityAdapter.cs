// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class SectionEntityAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section>
    {
        public SectionEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Dtos.Student.Section MapToType(Domain.Student.Entities.Section Source)
        {
            var sectionDto = new Ellucian.Colleague.Dtos.Student.Section();
            sectionDto.ActiveStudentIds = Source.ActiveStudentIds;
            sectionDto.AllowAudit = Source.AllowAudit;
            sectionDto.AllowPassNoPass = Source.AllowPassNoPass;
            sectionDto.Available = Source.Available;
            sectionDto.Books = new List<Ellucian.Colleague.Dtos.Student.SectionBook>();
            var bookAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionBook, Ellucian.Colleague.Dtos.Student.SectionBook>();
            var books = new List<Ellucian.Colleague.Dtos.Student.SectionBook>();
            foreach (var book in Source.Books)
            {
                books.Add(bookAdapter.MapToType(book));
            }
            sectionDto.Books = books;
            sectionDto.Capacity = Source.Capacity;
            sectionDto.Ceus = Source.Ceus;
            var coreqDtos = new List<Ellucian.Colleague.Dtos.Student.Corequisite>();
            sectionDto.CourseCorequisites = MapRequisitesToCourseCorequisiteDtos(Source.Requisites);
            sectionDto.CourseId = Source.CourseId;
            sectionDto.EndDate = Source.EndDate;
            sectionDto.FacultyIds = Source.FacultyIds;
            sectionDto.Id = Source.Id;
            sectionDto.IsActive = Source.IsActive;
            sectionDto.LearningProvider = Source.LearningProvider;
            sectionDto.LearningProviderSiteId = Source.LearningProviderSiteId;
            sectionDto.Location = Source.Location;
            sectionDto.MaximumCredits = Source.MaximumCredits;
            var meetingAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting>();
            var sectionMeetings = new List<Ellucian.Colleague.Dtos.Student.SectionMeeting>();
            foreach (var secMeeting in Source.Meetings)
            {
                sectionMeetings.Add(meetingAdapter.MapToType(secMeeting));
            }
            sectionDto.Meetings = sectionMeetings;
            sectionDto.MinimumCredits = Source.MinimumCredits;
            sectionDto.Number = Source.Number;
            sectionDto.OnlyPassNoPass = Source.OnlyPassNoPass;
            sectionDto.PrimarySectionId = Source.PrimarySectionId;
            sectionDto.SectionCorequisites = MapSectionRequisitesToSectionCorequisiteDtos(Source.SectionRequisites);
            sectionDto.StartDate = Source.StartDate;
            sectionDto.TermId = Source.TermId;
            sectionDto.Title = Source.Title;
            sectionDto.VariableCreditIncrement = Source.VariableCreditIncrement;
            sectionDto.WaitlistAvailable = Source.WaitlistAvailable;
            sectionDto.Waitlisted = Source.Waitlisted;
            
            return sectionDto;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Student.Corequisite> MapRequisitesToCourseCorequisiteDtos(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requisite> requisites)
        {
            var coreqDtos = new List<Dtos.Student.Corequisite>();
            foreach (var requisite in requisites)
            {
                // Create a corequisite for each requisite with a course Id (for a limited time)
                if (!string.IsNullOrEmpty(requisite.CorequisiteCourseId))
                {
                    var coreqDto = new Dtos.Student.Corequisite();
                    coreqDto.Id = requisite.CorequisiteCourseId;
                    coreqDto.Required = requisite.IsRequired;
                    coreqDtos.Add(coreqDto);
                }
            }
            return coreqDtos;
        }

        private IEnumerable<Ellucian.Colleague.Dtos.Student.Corequisite> MapSectionRequisitesToSectionCorequisiteDtos(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.SectionRequisite> sectionRequisites)
        {
            var coreqDtos = new List<Dtos.Student.Corequisite>();
            foreach (var requisite in sectionRequisites)
            {
                // Create a corequisite for each requisite with a course Id (for a limited time)
                if (requisite.CorequisiteSectionIds != null)
                {
                    // Create a corequiste for each section Id in a requisite with a list of section Ids
                    foreach (var sectionId in requisite.CorequisiteSectionIds)
                    {
                        var coreqDto = new Dtos.Student.Corequisite();
                        coreqDto.Id = sectionId;
                        coreqDto.Required = requisite.IsRequired;
                        coreqDtos.Add(coreqDto);
                    }
                }
            }
            return coreqDtos;
        }
    }
}