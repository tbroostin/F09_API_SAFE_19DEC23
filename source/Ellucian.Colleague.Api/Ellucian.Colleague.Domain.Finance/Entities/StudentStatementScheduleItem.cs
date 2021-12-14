// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Student course information for display in the Student Statement
    /// </summary>
    [Serializable]
    public class StudentStatementScheduleItem
    {
        private const string CourseDelimiter = "-";
        private const string SectionDatesDelimiter = "-";
        private const string ToBeDeterminedText = "TBD";

        private AcademicCredit _academicCredit;
        private Section _section;

        /// <summary>
        /// ID of the course section, i.e. "MATH-100-01"
        /// </summary>
        public string SectionId { get { return _academicCredit.CourseName + CourseDelimiter + _academicCredit.SectionNumber; } }

        /// <summary>
        /// Title of the course section
        /// </summary>
        public string SectionTitle { get { return _academicCredit.Title; } }

        /// <summary>
        /// Term description for the course section
        /// </summary>
        public string SectionTerm { get; set; }

        /// <summary>
        /// Number of credits for the course section
        /// </summary>
        public decimal Credits { get { return _academicCredit.Credit; } }

        /// <summary
        /// Number of continuing education units for the course section
        /// </summary>
        public decimal ContinuingEducationUnits { get { return _academicCredit.ContinuingEducationUnits; } }

        /// <summary>
        /// Concatenation of newline-delimited groupings of meeting days for the course section
        /// </summary>
        public string MeetingDays
        {
            get
            {
                List<string> meetingDays = new List<string>();
                if (_section.Meetings.Any())
                {
                    foreach (var sectionMeeting in _section.Meetings)
                    {
                        if (sectionMeeting.Days.Count > 0)
                        {
                            meetingDays.Add(ConvertSectionMeetingDaysCollectionToString(sectionMeeting.Days));
                        }
                        else
                        {
                            meetingDays.Add(ToBeDeterminedText);
                        }
                    }
                    return String.Join(Environment.NewLine, meetingDays.ToArray());
                }
                else
                {
                    return ToBeDeterminedText;
                }
            }
        }

        /// <summary>
        /// Concatenation of newline-delimited groupings of meeting times for the course section
        /// </summary>
        public string MeetingTimes
        {
            get
            {
                List<string> meetingTimes = new List<string>();
                if (_section.Meetings.Any())
                {
                    foreach (var sectionMeeting in _section.Meetings)
                    {
                        meetingTimes.Add(ConvertSectionMeetingTimesToString(sectionMeeting));
                    }
                    return String.Join(Environment.NewLine, meetingTimes.ToArray());
                }
                else
                {
                    return ToBeDeterminedText;
                }

            }
        }

        /// <summary>
        /// Concatenation of newline-delimited groupings of meeting locations for the course section
        /// </summary>
        public string MeetingLocations
        {
            get
            {
                List<string> meetingLocations = new List<string>();
                if (_section.Meetings.Any())
                {
                    foreach (var sectionMeeting in _section.Meetings)
                    {
                        StringBuilder sectionLocation = new StringBuilder();
                        if (!String.IsNullOrEmpty(_section.Location))
                        {
                            sectionLocation.Append(_section.Location + " ");
                        }
                        if (!String.IsNullOrEmpty(sectionMeeting.Room))
                        {
                            sectionLocation.Append(sectionMeeting.Room.Replace('*', ' '));
                        }
                        else
                        {
                            sectionLocation.Append(ToBeDeterminedText);
                        }
                        meetingLocations.Add(sectionLocation.ToString());
                    }
                    return String.Join(Environment.NewLine, meetingLocations.ToArray());
                }
                return ToBeDeterminedText;
            }
        }

        /// <summary>
        /// Concatenation of the course section start and end dates
        /// </summary>
        public string SectionDates
        {
            get
            {
                return ConvertSectionDatesToString(_section);
            }
        }

        /// <summary>
        /// Constructor for StudentStatementScheduleItem
        /// </summary>
        /// <param name="academicCredit">Details of a course section for a student</param>
        /// <param name="section">General course section information</param>
        public StudentStatementScheduleItem(AcademicCredit academicCredit, Section section)
        {
            if (academicCredit == null)
            {
                throw new ArgumentNullException("academicCredit", "Academic credit cannot be null.");
            }
            if (section == null)
            {
                throw new ArgumentNullException("section", "Section cannot be null.");
            }
            if (section.Id != academicCredit.SectionId)
            {
                throw new ArgumentException("section.Id", "Academic credit section ID and section ID must match.");
            }

            _academicCredit = academicCredit;
            _section = section;
        }

        private string ConvertSectionMeetingDaysCollectionToString(IEnumerable<DayOfWeek> collection, string separator = "")
        {
            List<string> dow = new List<string>();
            if (collection.Contains(DayOfWeek.Monday))
            {
                dow.Add("M");
            }
            if (collection.Contains(DayOfWeek.Tuesday))
            {
                dow.Add("Tu");
            }
            if (collection.Contains(DayOfWeek.Wednesday))
            {
                dow.Add("W");
            }
            if (collection.Contains(DayOfWeek.Thursday))
            {
                dow.Add("Th");
            }
            if (collection.Contains(DayOfWeek.Friday))
            {
                dow.Add("F");
            }
            if (collection.Contains(DayOfWeek.Saturday))
            {
                dow.Add("Sa");
            }
            if (collection.Contains(DayOfWeek.Sunday))
            {
                dow.Add("Su");
            }
            return string.Join(separator, dow.ToArray());
        }

        private string ConvertSectionMeetingTimesToString(SectionMeeting sectionMeeting)
        {
            StringBuilder meetingTime = new StringBuilder();
            if (!sectionMeeting.StartTime.HasValue)
            {
                if (!sectionMeeting.EndTime.HasValue)
                {
                    // Show TBA if the course section meeting has no start or end time
                    meetingTime.Append(ToBeDeterminedText);
                }
                else
                {
                    string endTimeString = sectionMeeting.EndTime.Value.LocalDateTime.ToString("t");
                    string endTimeAmOrPm = endTimeString.Substring((endTimeString.Length - 2));
                    meetingTime.Append(ToBeDeterminedText + "- " + endTimeString);
                }
            }
            else
            {
                string startTimeString = sectionMeeting.StartTime.Value.LocalDateTime.ToString("t");
                string startTimeAmOrPm = startTimeString.Substring((startTimeString.Length - 2));
                if (!sectionMeeting.EndTime.HasValue)
                {
                    meetingTime.Append(startTimeString + "-" + ToBeDeterminedText);
                }
                else
                {
                    string endTimeString = sectionMeeting.EndTime.Value.LocalDateTime.ToString("t");
                    string endTimeAmOrPm = endTimeString.Substring((endTimeString.Length - 2));
                    if (startTimeAmOrPm != endTimeAmOrPm)
                    {
                        meetingTime.Append(startTimeString + "-" + endTimeString);
                    }
                    else
                    {
                        meetingTime.Append(startTimeString.Substring(0, startTimeString.Length - 3) + "-" + endTimeString);
                    }
                }
            }
            return meetingTime.ToString();
        }

        private string ConvertSectionDatesToString(Section section)
        {
            var dateRange = new StringBuilder();
            if (section.FirstMeetingDate != null)
            {
                dateRange.Append(section.FirstMeetingDate.Value.ToShortDateString());
            }
            if (section.FirstMeetingDate == null)
            {
                dateRange.Append(section.StartDate.ToShortDateString());
            }
            dateRange.Append(SectionDatesDelimiter);
            if (section.EndDate == null && section.LastMeetingDate == null)
            {
                dateRange.Append(ToBeDeterminedText);
            }
            if (section.LastMeetingDate != null)
            {
                dateRange.Append(section.LastMeetingDate.Value.ToShortDateString());
            }
            if (section.LastMeetingDate == null) 
            {
                dateRange.Append(section.EndDate.Value.ToShortDateString());
            }
            return dateRange.ToString();
        }
    }
}
