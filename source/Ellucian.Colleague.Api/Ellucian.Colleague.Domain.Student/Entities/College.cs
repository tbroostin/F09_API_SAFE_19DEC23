// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class College
    {
        private readonly string _collegeId;
        public string CollegeId { get { return _collegeId; } }
        public string CollegeName { get; set; }
        public decimal? Gpa { get; set; }
        public decimal? SummaryCredits { get; set; }
        public string LastAttendedYear { get; set; }
        public List<Credential> Credentials { get; set; }
        public DateTime? CredentialsEndDate { get; set; }
        public string Comments { get; set; }

        public College(string collegeId, decimal? collegeGpa, string lastAttendedYear)
        {
            if (string.IsNullOrEmpty(collegeId))
            {
                throw new ArgumentNullException("collegeId", "CollegeId must not be null or empty");
            }
            this._collegeId = collegeId;
            this.Gpa = collegeGpa;
            this.LastAttendedYear = lastAttendedYear;
            this.Credentials = new List<Credential>();
        }
        public College(string collegeId)
        {
            if (string.IsNullOrEmpty(collegeId))
            {
                throw new ArgumentNullException("collegeId", "CollegeId must not be null or empty");
            }
            this._collegeId = collegeId;
            this.Credentials = new List<Credential>();
        }
    }
}
