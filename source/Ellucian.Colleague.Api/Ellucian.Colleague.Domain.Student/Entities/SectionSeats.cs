// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section seats information. Light weight class to only carry section's real time seats details.
    /// </summary>
    [Serializable]
    public class SectionSeats : SeatsManagement
    {
        /// <summary>
        /// Section Constructor. Section inherits course name (Course.Subject.Code + Course.Number)
        /// </summary>
        /// <param name="id">Section ID</param>
        /// <param name="allowWaitlist">Students may put themselves on a wait list</param>
        /// <param name="waitlistClosed">The wait list for this section is closed</param>
        /// <param name="isSeatServiceEnabled">Indicates whether the external seat service is enabled</param>
        /// <param name="useSeatServiceWhenEnabled">Indicates whether the external seat service should be used for section seat counts
        /// NOTE: Ethos endpoints should always set this parameter to false as Ethos is used to sync data between the local database and the external seat service</param>
        public SectionSeats(string id, bool allowWaitlist = false, bool waitlistClosed = false,
            bool isSeatServiceEnabled = false, bool useSeatServiceWhenEnabled = false)
        {
            _Id = id;
            _AllowWaitlist = allowWaitlist;
            _WaitlistClosed = waitlistClosed;
            ActiveStudentIds = _ActiveStudentIds.AsReadOnly();
            Statuses = _Statuses.AsReadOnly();
            CrossListedSections = new List<SectionSeats>();
            IsSeatServiceEnabled = isSeatServiceEnabled;
            UseSeatServiceWhenEnabled = useSeatServiceWhenEnabled;
            AreSeatCountsAvailable = true;
        }

        private string _Id;
        /// <summary>
        /// Section ID
        /// </summary>
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Section Id cannot be changed");
                }
            }
        }

        private string _Guid;
        /// <summary>
        /// GUID for the section; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        /// <summary>
        /// Sections that are cross listed with this section
        /// </summary>
        public List<SectionSeats> CrossListedSections { get; private set; }

        //add list of active student ids
        public void AddActiveStudents(List<string> activeStudentIds)
        {
            if (activeStudentIds != null)
            {
                _ActiveStudentIds.AddRange(activeStudentIds);
            }
            else
            {
                throw new ArgumentOutOfRangeException("activeStudentIds", "activeStudentIds cannot be null.");
            }
        }
        //add list of section statuses
        public void AddStatuses(IEnumerable<SectionStatusItem> statuses)
        {
            if (statuses != null)
            {
                _Statuses.AddRange(statuses);
            }
            else
            {
                throw new ArgumentOutOfRangeException("statuses", "statuses cannot be null.");
            }
        }

        public void AddCrossListedSection(SectionSeats crossListedSection)
        {
            if (crossListedSection != null)
            {
                CrossListedSections.Add(crossListedSection);
            }
        }

        //overidden method to return cross listed sections
        protected override List<SeatsManagement> GetCrossListedSections()
        {
            List<SeatsManagement> seatsManagement = new List<SeatsManagement>();
            foreach (SectionSeats s in CrossListedSections)
            {
                seatsManagement.Add(s);
            }
            return seatsManagement;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            SectionSeats other = obj as SectionSeats;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
