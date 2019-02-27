// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The faculty assigned to a section or to a section meeting
    /// </summary>
    [Serializable]
    public class SectionFaculty
    {
        private string _id;
        /// <summary>
        /// Section faculty ID
        /// </summary>
        public string Id 
        { 
            get { return _id; }
            set
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    throw new InvalidOperationException("Cannot assign new value to ID.");
                }
                _id = value;
            }
        }
        private string _guid;
        /// <summary>
        /// Section faculty ID
        /// </summary>
        public string Guid
        {
            get { return _guid; }
            set
            {
                if (!string.IsNullOrEmpty(_guid))
                {
                    throw new InvalidOperationException("Cannot assign new value to GUID.");
                }
                _guid = value;
            }
        }

        private string _sectionId;
        /// <summary>
        /// Section ID
        /// </summary>
        public string SectionId
        {
            get { return _sectionId; }
            set
            {
                if (!string.IsNullOrEmpty(_sectionId))
                {
                    throw new InvalidOperationException("Cannot assign new value to section ID.");
                }
                _sectionId = value;
            }
        }

        private readonly string _facultyId;
        /// <summary>
        /// ID of faculty member
        /// </summary>
        public string FacultyId { get { return _facultyId; } }

        private readonly string _instructionalMethodCode;
        /// <summary>
        /// Instructional method code
        /// </summary>
        public string InstructionalMethodCode { get { return _instructionalMethodCode; } }

        private readonly DateTime _startDate;
        /// <summary>
        /// Date on which faculty member begins teaching this section
        /// </summary>
        public DateTime StartDate { get { return _startDate; } }

        private readonly DateTime _endDate;
        /// <summary>
        /// Date on which faculty member completes teaching this section
        /// </summary>
        public DateTime EndDate { get { return _endDate; } }

        private decimal _responsibilityPercentage;
        /// <summary>
        /// The percentage of responsibility of this faculty member for the section
        /// </summary>
        public decimal ResponsibilityPercentage 
        { 
            get { return _responsibilityPercentage; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "The responsibility percentage must be greater than or equal to zero.");
                }
                _responsibilityPercentage = value;
            }
        }

        /// <summary>
        /// Faculty load factor for all meetings with this instructional method
        /// </summary>
        public decimal? LoadFactor { get; set; }

        /// <summary>
        /// Pointer to the contract assignment in HR
        /// </summary>
        public string ContractAssignment { get; set; }

        /// <summary>
        /// Code indicating the type of teaching arrangement for the instructor
        /// </summary>
        public string TeachingArrangementCode { get; set; }

        /// <summary>
        /// Total number of minutes this faculty member will be in a section
        /// </summary>
        public int TotalMeetingMinutes { get; set; }

        /// <summary>
        /// Calculated load factor for this meeting and faculty
        /// </summary>
        public decimal MeetingLoadFactor { get; set; }

        /// <summary>
        /// Used by Ethos API to indicate if this is the primary instructor
        /// for the section.
        /// </summary>
        public bool PrimaryIndicator { get; set; }

        /// <summary>
        /// Used by Ethos API to point to section meeting with the same
        /// instructional method (should be an array and will be in future versions).
        /// </summary>
        public List<string> SecMeetingIds { get;  set; }

        /// <summary>
        /// Section Faculty Constructor
        /// </summary>
        /// <param name="id">Section faculty ID</param>
        /// <param name="sectionId">Section ID</param>
        /// <param name="facultyId">Faculty ID</param>
        /// <param name="instrMethod">Instructional Method Code</param>
        /// <param name="startDate">Starting date</param>
        /// <param name="endDate">Ending date</param>
        /// <param name="respPercent">Responsibility percentage</param>
        public SectionFaculty(string id, string sectionId, string facultyId, string instrMethod, DateTime startDate, DateTime endDate, decimal respPercent)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Faculty ID is required.");
            }
            if (string.IsNullOrEmpty(instrMethod))
            {
                throw new ArgumentNullException("instrMethod", "Instructional Method code is required.");
            }
            if (startDate == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("startDate", string.Format("Section faculty record {0} for section {1} missing faculty start date for faculty {2}", id, sectionId, facultyId));
            }
            if (endDate == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("endDate", string.Format("Section faculty record {0} for section {1} missing faculty end date for faculty {2}", id, sectionId, facultyId));
            }
            if (respPercent < 0)
            {
                throw new ArgumentOutOfRangeException("respPercent", "The responsibility percentage must be greater than or equal to zero.");
            }

            _id = id;
            _sectionId = sectionId;
            _facultyId = facultyId;
            _instructionalMethodCode = instrMethod;
            _startDate = startDate;
            _endDate = endDate;
            _responsibilityPercentage = respPercent;
        }

        /// <summary>
        /// Section Faculty Constructor
        /// </summary>
        /// <param name="id">Section faculty ID</param>
        /// <param name="sectionId">Section ID</param>
        /// <param name="facultyId">Faculty ID</param>
        /// <param name="instrMethod">Instructional Method Code</param>
        /// <param name="startDate">Starting date</param>
        /// <param name="endDate">Ending date</param>
        /// <param name="respPercent">Responsibility percentage</param>
        public SectionFaculty(string guid, string id, string sectionId, string facultyId, string instrMethod, DateTime startDate, DateTime endDate, decimal respPercent)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", string.Format("Error processing section faculty record {0} for section {1}: missing guid", id, sectionId));
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", string.Format("Error processing section faculty record {0} for section {1} missing ID", guid, sectionId));
            }
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", string.Format("Error processing section faculty record {0} for section {1}: missing faculty ID ", id, sectionId));
            }
            if (string.IsNullOrEmpty(instrMethod))
            {
                throw new ArgumentNullException("instrMethod", string.Format("Section faculty record {0} for section {1} missing instructional method", id, sectionId));
            }
            if (startDate == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("startDate", string.Format("Section faculty record {0} for section {1} missing faculty start date for faculty {2}", id, sectionId, facultyId));
            }
            if (endDate == default(DateTime))
            {
                throw new ArgumentOutOfRangeException("endDate", string.Format("Section meeting record {0} for section {1} missing faculty end date for faculty {2}", id, sectionId, facultyId));
            }
            if (respPercent < 0)
            {
                throw new ArgumentOutOfRangeException("respPercent", string.Format("Section faculty record {0} for section {1}: The responsibility percentage must be greater than or equal to zero for faculty {2}", id, sectionId, facultyId));
            }

            _guid = guid;
            _id = id;
            _sectionId = sectionId;
            _facultyId = facultyId;
            _instructionalMethodCode = instrMethod;
            _startDate = startDate;
            _endDate = endDate;
            _responsibilityPercentage = respPercent;
            SecMeetingIds = new List<string>();
        }
    }
}
