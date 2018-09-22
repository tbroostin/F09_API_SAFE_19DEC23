// Copyright 2016 Ellucian Company L.P. and it's affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student Academic Level Cohort
    /// </summary>
    [Serializable]
    public class StudentAcademicLevelCohort
    {
        private readonly string _otherCohortGroup;
        private readonly DateTime? _otherCohortStartDate;
        private readonly DateTime? _otherCohortEndDate;

        /// <summary>
        /// Other Cohort Group
        /// </summary>
        public string OtherCohortGroup
        {
            get { return _otherCohortGroup; }
        }

        /// <summary>
        /// Other Cohort StartDate
        /// </summary>
        public DateTime? OtherCohortStartDate
        {
            get { return _otherCohortStartDate; }
        }

        /// <summary>
        /// Other Cohort EndDate
        /// </summary>
        public DateTime? OtherCohortEndDate
        {
            get { return _otherCohortEndDate; }
        }

        /// <summary>
        /// Initialize the StudentAcademicLevelCohort
        /// </summary>

        public StudentAcademicLevelCohort(string cohortGroup, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(cohortGroup))
            {
                throw new ArgumentNullException("cohortGroup");
            }
            if (startDate == null || startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate");
            }
            _otherCohortGroup = cohortGroup;
            _otherCohortStartDate = startDate;
            _otherCohortEndDate = endDate;
        }
    }
}