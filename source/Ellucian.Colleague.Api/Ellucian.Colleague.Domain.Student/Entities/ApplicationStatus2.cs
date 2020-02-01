//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ApplicationStatus2
    {
        public ApplicationStatus2(string guid, string applicantKey, string decisionType, DateTime decidedOnDate, DateTime decidedOnTime)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Admission decision guid is required.");
            }
            if (string.IsNullOrEmpty(applicantKey))
            {
                throw new ArgumentNullException("Admission decision record key is required.");
            }
            if(string.IsNullOrEmpty(decisionType))
            {
                throw new ArgumentNullException("Admission decision type is required.");
            }
            if (decidedOnDate == null)
            {
                throw new ArgumentNullException("Admission decision decided on date is required.");
            }
            if (decidedOnTime == null)
            {
                throw new ArgumentNullException("Admission decision decided on time is required.");
            }

            Guid = guid;
            ApplicantRecordKey = applicantKey;
            DecisionType = decisionType;
            DecidedOnDate = decidedOnDate;
            DecidedOnTime = decidedOnTime;
        }

        public string Guid { get; private set; }
        public string ApplicantRecordKey { get; private set; }
        //public string Application { get; private set; }
        public string DecisionType { get; private set; }
        public DateTime DecidedOnDate { get; private set; }
        public DateTime DecidedOnTime { get; private set; }

        public DateTimeOffset DecidedOn { get; set; }

    }
}
