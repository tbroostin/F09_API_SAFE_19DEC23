// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class MidTermGrade
    {
        private int _Position;
        private string _GradeId;
        private DateTimeOffset? _GradeTimestamp;
        private string _submittedBy;

        /// <summary>
        /// Relative position of this grade in the list of midterm grades.
        /// </summary>
        public int Position { get { return _Position; } }

        /// <summary>
        /// ID of the grade given
        /// </summary>
        public string GradeId { get { return _GradeId; } }

        /// <summary>
        /// Colleague ID of the grade given
        /// </summary>
        public string GradeKey { get; set; }

        /// <summary>
        /// Letter Grade like A, B, C, F etc.
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Date/time stamp of the grade given
        /// </summary>
        public DateTimeOffset? GradeTimestamp { get { return _GradeTimestamp; } }

        /// <summary>
        /// Grade submittedby
        /// </summary>
        public string SubmittedBy { get { return _submittedBy; } }

        /// <summary>
        /// The reason for the  grade submission
        /// </summary>
        public string GradeChangeReason { get; set; }

        /// <summary>
        /// Grade typecode 
        /// </summary>
        public string GradeTypeCode { get; set; }

        /// <summary>
        /// Constructor for midterm grade
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id"></param>
        /// <param name="timestamp"></param>
        public MidTermGrade(int position, string id, DateTimeOffset? timestamp)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            if (position < 1 || position > 6) throw new ArgumentException("invalid position");
            // timestamp may be null if the entry pre-dates release which timestamps
            _Position = position;
            _GradeId = id;
            _GradeTimestamp = timestamp;
        }

        /// <summary>
        /// Constructor for midterm grade
        /// </summary>
        /// <param name="position"></param>
        /// <param name="id"></param>
        /// <param name="timestamp"></param>
        /// <param name="submittedBy"></param>
        /// <param name="submittedOn"></param>
        public MidTermGrade(int position, string id, DateTimeOffset? timestamp, string submittedBy)
            : this(position, id, timestamp)
        {
            _submittedBy = submittedBy;
        }

        /// <summary>
        /// Equals method required for object comparison
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            MidTermGrade other = obj as MidTermGrade;
            if (other == null) {
                return false;
            }
            if (other.Position.Equals(_Position) &&
                other.GradeId.Equals(_GradeId) &&
                (other.GradeTimestamp.HasValue && _GradeTimestamp.HasValue && other.GradeTimestamp.Equals(_GradeTimestamp))) {
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// HashCode method required for comparison.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            string dateTime = "";
            if (_GradeTimestamp.HasValue) { dateTime = _GradeTimestamp.ToString(); }
            string sPos = _Position.ToString();
            string all = sPos + _GradeId + dateTime;
            return all.GetHashCode();
        }

    }
}
