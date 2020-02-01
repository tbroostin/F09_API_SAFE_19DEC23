// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A completed advisement for a student for a given date and time by a given advisor
    /// </summary>
    [Serializable]
    public class CompletedAdvisement
    {
        private DateTime _completionDate;
        /// <summary>
        /// Date on which an advisor is marking the student's advisement complete
        /// </summary>
        public DateTime CompletionDate { get { return _completionDate; } }

        private DateTimeOffset _completionTime;
        /// <summary>
        /// Time of day at which an advisor is marking the student's advisement complete
        /// </summary>
        public DateTimeOffset CompletionTime { get { return _completionTime; } }

        private string _advisorId;
        /// <summary>
        /// ID of the advisor who is marking the student's advisement complete
        /// </summary>
        public string AdvisorId { get { return _advisorId; } }

        /// <summary>
        /// Creates a new instance of the <see cref="CompletedAdvisement"/> class.
        /// </summary>
        /// <param name="completionDate">Date on which an advisor is marking the student's advisement complete</param>
        /// <param name="completionTime">Time of day at which an advisor is marking the student's advisement complete</param>
        /// <param name="advisorId">ID of the advisor who is marking the student's advisement complete</param>
        public CompletedAdvisement(DateTime completionDate, DateTimeOffset completionTime, string advisorId)
        {
            if (string.IsNullOrEmpty(advisorId))
            {
                throw new ArgumentNullException("advisorId", "An advisor ID must be specified when creating a completed advisement.");
            }
            _completionDate = completionDate;
            _completionTime = completionTime;
            _advisorId = advisorId;
        }
    }
}
