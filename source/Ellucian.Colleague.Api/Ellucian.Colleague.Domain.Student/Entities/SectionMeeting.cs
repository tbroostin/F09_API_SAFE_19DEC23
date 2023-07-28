// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A meeting time contains information that describes when a section is offered. 
    /// </summary>
    [Serializable]
    public class SectionMeeting: IComparable<SectionMeeting>
    {
        /// <summary>
        /// Section meeting ID
        /// </summary>
        private string _id;
        public string Id 
        { 
            get { return _id; }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _id = value;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Id.");
                }
            }
        }

        /// <summary>
        /// Globally unique ID (GUID)
        /// </summary>
        private string _guid;
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (string.IsNullOrEmpty(_guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        /// <summary>
        /// ID of the section this meeting is linked to
        /// </summary>
        private string _sectionId;
        public string SectionId 
        { 
            get { return _sectionId; }
            set
            {
                if (string.IsNullOrEmpty(_sectionId))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _sectionId = value;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of SectionId.");
                }
            }
        }

        /// <summary>
        /// Meeting time instructional method (such as lecture, lab). It is required.
        /// </summary>
        private readonly string _InstructionalMethodCode;
        public string InstructionalMethodCode { get { return _InstructionalMethodCode; } }

        /// <summary>
        /// Start date of meeting pattern
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// End date of meeting pattern
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Frequency indicator for meeting pattern (daily, weekly, etc)
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// Meeting time start time
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Meeting time start time
        /// </summary>
        public DateTime? RawStartTime { get; set; }

        /// <summary>
        /// Meeting time end time 
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Days of the week (enumerable) when this meeting time occurs.
        /// </summary>
        public List<DayOfWeek> Days { get; set; }

        /// <summary>
        /// Room number where this meeting time occurs. Includes the building code 
        /// </summary>
        public string Room { get; set; }

        /// <summary>
        /// Amount this section meeting counts toward a faculty member's workload
        /// </summary>
        public decimal? Load { get; set; }

        /// <summary>
        /// Total number of minutes this meeting time will meet
        /// </summary>
        public int TotalMeetingMinutes { get; set; }

        /// <summary>
        /// Indicates whether this meeting has an on-line type of instructional method.
        /// </summary>
        public bool IsOnline { get; set; }

        private readonly List<string> _facultyIds = new List<string>();
        /// <summary>
        /// The list of faculty teaching this meeting; if empty, then all faculty assigned to the
        /// section are teaching this meeting.
        /// </summary>
        public ReadOnlyCollection<string> FacultyIds { get; private set; }

        private List<SectionFaculty> _facultyRoster = new List<SectionFaculty>();
        /// <summary>
        /// The list of faculty associated with this section meeting
        /// </summary>
        /// <remarks>Colleague doesn't currently represent section faculty in this manner, but it's the way the CDM is defined.</remarks>
        public ReadOnlyCollection<SectionFaculty> FacultyRoster { get; private set; }

        /// <summary>
        /// Override any room availability conflicts
        /// </summary>
        public bool OverrideRoomAvailability { get; set; }

        /// <summary>
        /// Override any room capacity conflicts
        /// </summary>
        public bool OverrideRoomCapacity { get; set; }

        /// <summary>
        /// Override any faculty availability conflicts
        /// </summary>
        public bool OverrideFacultyAvailability { get; set; }

        /// <summary>
        /// Override any faculty capacity conflicts
        /// </summary>
        public bool OverrideFacultyCapacity { get; set; }

        /// <summary>
        /// Meeting constructor. Instructional method, start date, end date, frequency all required.
        /// </summary>
        /// <param name="id">ID of source record</param>
        /// <param name="sectionId">ID of related section</param>
        /// <param name="instMethodCode">Type of instruction</param>
        /// <param name="startDate">Start date for this meeting pattern</param>
        /// <param name="endDate">End date for this meeting pattern</param>
        /// <param name="frequency">Frequency of meeting pattern, not required</param>
        public SectionMeeting(string id, string sectionId, string instMethodCode, DateTime? startDate, DateTime? endDate, string frequency)
        {
            if (string.IsNullOrEmpty(instMethodCode))
            {
                throw new ArgumentNullException("instMethodCode", "Instructional Method Code must be provided");
            }
            // No error for null/empty frequency

            _id = id;
            _sectionId = sectionId;

            _InstructionalMethodCode = instMethodCode;
            StartDate = startDate;
            EndDate = endDate;
            Frequency = frequency;
            Days = new List<DayOfWeek>();

            FacultyRoster = _facultyRoster.AsReadOnly();
            FacultyIds = _facultyIds.AsReadOnly();
        }

        /// <summary>
        /// Add a faculty member to this section meeting
        /// </summary>
        /// <param name="sectionFaculty">Section faculty info</param>
        public void AddSectionFaculty(SectionFaculty sectionFaculty)
        {
            if (sectionFaculty == null)
            {
                throw new ArgumentNullException("sectionFaculty", "Section faculty cannot be null");
            }
            // Ignore duplicates - faculty ID, start date, and end date are all the same
            var duplicate = FacultyRoster.FirstOrDefault(x => x.FacultyId == sectionFaculty.FacultyId && x.StartDate == sectionFaculty.StartDate && x.EndDate == sectionFaculty.EndDate);
            if (duplicate == null)
            {
                _facultyRoster.Add(sectionFaculty);
            }
        }

        /// <summary>
        /// Remove section faculty from this section meeting
        /// </summary>
        /// <param name="sectionFaculty">The section faculty to remove</param>
        public void RemoveSectionFaculty(SectionFaculty sectionFaculty)
        {
            if (sectionFaculty == null)
            {
                throw new ArgumentNullException("sectionFaculty", "Section faculty cannot be null");
            }
            // Find the section faculty that is to be removed
            int idx = _facultyRoster.FindIndex(x => x.Id == sectionFaculty.Id);
            if (idx < 0)
            {
                throw new InvalidOperationException("Section faculty " + sectionFaculty.Id + " is not part of section meeting " + Id);
            }
            _facultyRoster.RemoveAt(idx);
        }

        /// <summary>
        /// Add a faculty ID to this meeting
        /// </summary>
        /// <param name="facultyId">Faculty ID</param>
        public void AddFacultyId(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId");
            }
            if (!_facultyIds.Contains(facultyId))
            {
                _facultyIds.Add(facultyId);
            }
        }

        /// <summary>
        /// Remove section faculty from this section meeting
        /// </summary>
        /// <param name="sectionFaculty">The section faculty to remove</param>
        public void RemoveFacultyId(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Section faculty cannot be null");
            }
            // Find the section faculty that is to be removed
            if (_facultyIds.Contains(facultyId))
            {
                _facultyIds.Remove(facultyId);
            }
        }

        /// <summary>
        /// Add a group of faculty IDs to this meeting
        /// </summary>
        /// <param name="facultyIds">List of faculty IDs</param>
        public void AddFacultyIds(IEnumerable<string> facultyIds)
        {
            if (facultyIds == null || facultyIds.Count() == 0)
            {
                throw new ArgumentNullException("facultyIds");
            }
            foreach (string id in facultyIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    AddFacultyId(id);
                }
            }
        }
        /// <summary>
        /// Compariosn method to compare section meetings so that sections can be sorted on meetings
        /// </summary>
        /// <param name="compareWith"></param>
        /// <returns></returns>
        public int CompareTo(SectionMeeting compareWith)
        {
            // Date, Instructional Method, Days, Time, Building, Room - in sequence comparison
            //compare start date
            if (this.StartDate.HasValue && compareWith.StartDate.HasValue)
            {
                if (this.StartDate.Value > compareWith.StartDate.Value)
                {
                    return 1;

                }
                else if (this.StartDate.Value < compareWith.StartDate.Value)
                {
                    return -1;
                }
            }

            if (this.StartDate.HasValue && !compareWith.StartDate.HasValue)
            {
                return -1;
            }
            if (!this.StartDate.HasValue && compareWith.StartDate.HasValue)
            {
                return 1;
            }

            //compare insructional methods if dates match or are null
            var instructionMethodCompare = this.InstructionalMethodCode.CompareTo(compareWith.InstructionalMethodCode);
            if (instructionMethodCompare != 0)
            {
                return instructionMethodCompare;
            }
            //now compare days since instrictional methods and dates were same. Days are enum values, simple comparison of each item in Days collection will work
            //We only need to compare the count of elements in both the collection, otherwise we assume days are equal.

            
            if((this.Days==null || this.Days.Count ==0) && (compareWith.Days!=null && compareWith.Days.Count>0))
            {
                return -1;
            }
            if ((this.Days != null && this.Days.Count > 0) && (compareWith.Days == null || compareWith.Days.Count == 0))
            {
                return 1;
            }
            int countOfDaysthis = 0;
            int countOfDaysInFirstMeeting = 0;
            int countOfDaysInOtherMeeting = 0;
            if (this.Days != null)
            {
                countOfDaysInFirstMeeting = this.Days.Count;
            }
            if (compareWith.Days != null)
            {
                countOfDaysInOtherMeeting = compareWith.Days.Count;
            }
            if (countOfDaysInFirstMeeting < countOfDaysInOtherMeeting)
            {
                countOfDaysthis = countOfDaysInFirstMeeting;
            }
            else
            {
                countOfDaysthis = countOfDaysInOtherMeeting;
            }
            for (int i = 0; i < countOfDaysthis; i++)
            {
                if (this.Days[i] > compareWith.Days[i])
                {
                    return 1;
                }
                if (this.Days[i] < compareWith.Days[i])
                {
                    return -1;
                }
            }
            //here days, instructional method, dates are same. Now check the start time 
            if (this.StartTime > compareWith.StartTime)
            {
                return 1;
            }
            else if (this.StartTime < compareWith.StartTime)
            {
                return -1;
            }
            // //here start time,  days, instructional method, dates are same. Now check the bulidings and rooms (like BLDG*ROOM)
            if (string.Compare(this.Room, compareWith.Room) > 0)
            {
                return 1;
            }
            else if (string.Compare(this.Room, compareWith.Room) < 0)
            {
                return -1;
            }
            //when reached here it means everythig matches therefore section meeting is same.
            return 0;
        }
    }
}
