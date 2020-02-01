// Copyright 2019 Ellucian Company L.P. and its acffiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A record of a faculty member indicating that grading is complete.   
    /// </summary>
    [Serializable]
    public class GradingCompleteIndication
    {
        /// <summary>
        /// The person ID of the faculty member that indicated that grading is complete.
        /// </summary>
        private string _completeOperator;
        public string CompleteOperator { get { return _completeOperator; } }
        
        /// <summary>
        /// The date and time on which the faculty member indicated that grading is complete.
        /// </summary>
        private DateTimeOffset _dateAndTime;
        public DateTimeOffset DateAndTime { get { return _dateAndTime; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="completeOperator">The person ID of the faculty member that indicated that grading is complete.</param>
        /// <param name="dateAndTime">The date and time on which the faculty member indicated that grading is complete.</param>
        public GradingCompleteIndication(string completeOperator, DateTimeOffset dateAndTime)
        {
            if (string.IsNullOrEmpty(completeOperator))
            {
                throw new ArgumentNullException("completeOperator");
            }

            if (dateAndTime == null)
            {
                throw new ArgumentNullException("dateAndTime");
            }

            _completeOperator = completeOperator;
            _dateAndTime = dateAndTime;

        }
    }
}
