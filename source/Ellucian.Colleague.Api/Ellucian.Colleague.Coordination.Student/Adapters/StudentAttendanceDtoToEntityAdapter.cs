// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using slf4net;
using System;


namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class StudentAttendanceDtoToEntityAdapter : BaseAdapter<Dtos.Student.StudentAttendance, StudentAttendance>
    {
        public StudentAttendanceDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        public override StudentAttendance MapToType(Dtos.Student.StudentAttendance source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "The source student attendance can not be null.");
            }

            var studentAttendanceEntity = new StudentAttendance(source.StudentId, source.SectionId, source.MeetingDate, source.AttendanceCategoryCode, source.MinutesAttended, source.Comment)
            {
                StudentCourseSectionId = source.StudentCourseSectionId,
                StartTime = source.StartTime,
                EndTime = source.EndTime,
                InstructionalMethod = source.InstructionalMethod                
            };
            return studentAttendanceEntity;
        }
    }
}
