// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using System;


namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Maps a SectionAttendance DTO to a SectionAttendance domain entity.
    /// </summary>
    public class SectionAttendanceDtoToEntityAdapter : BaseAdapter<Ellucian.Colleague.Dtos.Student.SectionAttendance, Ellucian.Colleague.Domain.Student.Entities.SectionAttendance>
    {
        /// <summary>
        /// Initializes a new instance of the SectionAttendance Dto to Entity Adapter class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SectionAttendanceDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override SectionAttendance MapToType(Dtos.Student.SectionAttendance source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source section attendance can not be null.");
            }
            try
            {
                if (source.MeetingInstance == null)
                {
                    throw new ArgumentException("source", "The source section attendance must contain a valid meeting instance");
                }
                var meetingInstance = new SectionMeetingInstance(source.MeetingInstance.Id, source.MeetingInstance.SectionId, source.MeetingInstance.MeetingDate, source.MeetingInstance.StartTime, source.MeetingInstance.EndTime);
                meetingInstance.InstructionalMethod = source.MeetingInstance.InstructionalMethod;
                var sectionAttendanceEntity = new SectionAttendance(source.SectionId, meetingInstance);
                foreach (var ssa in source.StudentAttendances)
                {
                    var studentSectionAttendance = new StudentSectionAttendance(ssa.StudentCourseSectionId, ssa.AttendanceCategoryCode, ssa.MinutesAttended, ssa.Comment);
                    sectionAttendanceEntity.AddStudentSectionAttendance(studentSectionAttendance);
                }
                return sectionAttendanceEntity;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to map SectionAttendance DTO to a valid SectionAttendance Entity.");
                throw;
            }

        }
    }
}
