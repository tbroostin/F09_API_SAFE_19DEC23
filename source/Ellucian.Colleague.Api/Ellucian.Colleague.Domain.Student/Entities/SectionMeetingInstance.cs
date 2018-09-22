// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A specific class meeting for a section - i.e. a calendar instance
    /// </summary>
    [Serializable]
    public class SectionMeetingInstance
    {
        // Id of the section for this meeting instance 
        private string _Id;
        /// <summary>
        /// Id of the section meeting instance (Required)
        /// </summary>
        public string Id {  get { return _Id; } }
       

        // Id of the section for this meeting instance 
        private string _SectionId;
        /// <summary>
        /// Id of the section for this meeting instance (Required)
        /// </summary>
        public string SectionId { get { return _SectionId; } }

        // Date and time when this calendar event begins
        private DateTime _MeetingDate;
        /// <summary>
        /// Date of the meeting instance (Required)
        /// </summary>
        public DateTime MeetingDate { get { return _MeetingDate; } }

        // Time of day when this meeting instance begins
        private DateTimeOffset? _StartTime;
        /// <summary>
        /// Time of day when this meeting instance begins
        /// </summary>
        public DateTimeOffset? StartTime { get { return _StartTime; } }

        // Time of day when this calendar event ends
        private DateTimeOffset? _EndTime;
        /// <summary>
        /// Time of day when this calendar event ends
        /// </summary>
        public DateTimeOffset? EndTime { get { return _EndTime; } }

        /// <summary>
        /// (Optional) Instructional Method code associated to this meeting instance if applicable.
        /// </summary>
        public string InstructionalMethod { get; set; }
        /// <summary>
        /// Constructor for a Section Meeting Instance
        /// </summary>
        /// <param name="sectionId">Section Id</param>
        /// <param name="startTime">Start time of the instance</param>
        /// <param name="endTime">End time of the instance</param>
        public SectionMeetingInstance(string id, string sectionId, DateTime meetingDate, DateTimeOffset? startTime, DateTimeOffset? endTime)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section Id must be provided.");
            }
            if (meetingDate.Date == DateTime.MinValue)
            {
                throw new ArgumentNullException("meetingDate", "Meeting Date must include a valid date");
            }
            if (startTime == default(DateTimeOffset))
            {
                throw new ArgumentNullException("startTime", "Start time may not be the default date/time");
            }
            if (endTime == default(DateTimeOffset))
            {
                throw new ArgumentNullException("endTime", "End time may not be the default date/time");
            }
            if (startTime > endTime)
            {
                throw new ArgumentException("Start time cannot be later than end time");
            }
            _Id = id;
            _SectionId = sectionId;
            _MeetingDate = meetingDate;
            _StartTime = startTime;
            _EndTime = endTime;
        }

        public bool BelongsToStudentAttendance(StudentAttendance attendance)
        {
            if (attendance == null)
            {
                return false;
            }
            // Must have same meeting date, start time, end time and instructional method to match. 
            // Consider it a match regardless whether instructional method is null or empty
            var tempInstructionalMethod = this.InstructionalMethod ?? string.Empty;
            return this.StartTime == attendance.StartTime && this.EndTime == attendance.EndTime && this.MeetingDate == attendance.MeetingDate && tempInstructionalMethod.Equals(attendance.InstructionalMethod ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            SectionMeetingInstance sectionMeeting = obj as SectionMeetingInstance;
            return sectionMeeting.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
