using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class CourseApproval
    {
        private readonly string _statusCode;
        private readonly DateTime _statusDate;
        private readonly string _approvingAgencyId;
        private readonly string _approvingPersonId;
        private readonly DateTime _date;

        public string StatusCode { get { return _statusCode; } }
        public DateTime StatusDate { get { return _statusDate; } }
        public string ApprovingAgencyId { get { return _approvingAgencyId; } }
        public string ApprovingPersonId { get { return _approvingPersonId; } }
        public DateTime Date { get { return _date; } }

        public CourseStatus Status { get; set; }

        /// <summary>
        /// Constructor for a course approval
        /// </summary>
        /// <param name="statusCode">Course status code</param>
        /// <param name="statusDate">Status Date</param>
        /// <param name="approvingAgencyId">ID of the approving agency</param>
        /// <param name="approvingPersonId">ID of the approving person</param>
        /// <param name="date">Date on which approval was given</param>
        public CourseApproval(string statusCode, DateTime statusDate, string approvingAgencyId, string approvingPersonId, DateTime date)
        {
            if (string.IsNullOrEmpty(statusCode))
            {
                throw new ArgumentNullException("statusCode", "Status code must be supplied.");
            }

            if (string.IsNullOrEmpty(approvingAgencyId) && string.IsNullOrEmpty(approvingPersonId))
            {
                throw new ArgumentException("One of either approving agency ID or approving person ID must be supplied.");
            }

            _statusCode = statusCode;
            _statusDate = statusDate;
            _approvingAgencyId = approvingAgencyId;
            _approvingPersonId = approvingPersonId;
            _date = date;
        }
    }
}
