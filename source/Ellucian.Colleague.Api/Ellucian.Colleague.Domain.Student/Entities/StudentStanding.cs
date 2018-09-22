// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class StudentStanding
    {
        // Required Fields
        private string _Id;
        /// <summary>
        /// Student Standing ID
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
                    throw new InvalidOperationException("Student Standing Id cannot be changed");
                }
            }
        }

      

        private string _studentId { get; set; }
        public string StudentId { get { return _studentId; } }
        private string _standingCode { get; set; }
        public string StandingCode { get { return _standingCode; } }
        private DateTime _standingDate { get; set; }
        public DateTime StandingDate { get { return _standingDate; } }
        // Optional Fields
        public string Guid { get; set; }
        public string Program { get; set; }
        public string Level { get; set; }
        public string Term { get; set; }
        public StudentStandingType Type { get; set; }
        public string CalcStandingCode { get; set; }
        public string OverrideReason { get; set; }
        public bool IsCurrent { get; set; }

        public StudentStanding(string id, string studentId, string standing, DateTime standingDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            } 
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(standing))
            {
                throw new ArgumentNullException("standing");
            }
            if (standingDate == default(DateTime))
            {
                throw new ArgumentNullException("standingDate");
            }
            _Id = id;
            _studentId = studentId;
            _standingCode = standing;
            _standingDate = standingDate;
        }
    }
}
