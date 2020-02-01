// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    [Serializable]
    public class DegreePlanApproval
    {
        private DateTimeOffset _Date;
        private string _PersonId;
        private DegreePlanApprovalStatus _Status;
        private string _CourseId;
        private string _TermCode;

        /// <summary>
        /// Effective date/time
        /// </summary>
        public DateTimeOffset Date { get { return _Date; } }

        /// <summary>
        /// The person (student or advisor) who effected this state
        /// </summary>
        public string PersonId { get { return _PersonId; } }

        /// <summary>
        /// An enumeration value that describes the state of the approval
        /// </summary>
        public DegreePlanApprovalStatus Status { get { return _Status; } }

        /// <summary>
        /// The id of the course being approved.
        /// </summary>
        public string CourseId { get { return _CourseId; } }

        /// <summary>
        /// Term code for which the course is being approved. (Optional)
        /// </summary>
        public string TermCode { get { return _TermCode; } }

        /// <summary>
        /// DegreePlanApproval indicates the approval status of a degree plan planned course item.
        /// These items are added to the degree plan only by methods within the degree plan.
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="termCode"></param>
        /// <param name="status"></param>
        public DegreePlanApproval(string personId, DegreePlanApprovalStatus status, DateTimeOffset date, string courseId, string termCode)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person id is required for a degree plan approval");
            }
            if (date == null)
            {
                throw new ArgumentNullException("date");
            }
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId", "Course id is required for a degree plan approval");
            }
            if (string.IsNullOrEmpty(termCode))
            {
                throw new ArgumentNullException("termCode", "Term code is required for a degree plan approval");
            }
            _PersonId = personId;
            _Status = status;
            _Date = date;
            _CourseId = courseId;
            _TermCode = termCode;
        }
    }


}
