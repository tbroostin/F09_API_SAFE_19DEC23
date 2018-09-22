// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This is a transaction posted to the General Ledger through the Data Model.
    /// </summary>
    [Serializable]
    public class ProjectCF
    {
        private readonly string referenceCode;

        public string ReferenceCode
        {
            get { return referenceCode; }
        }

        private readonly DateTime? startOn;

        public DateTime? StartOn
        {
            get { return startOn; }
        }

        private readonly string recordKey;

        public string RecordKey
        {
            get { return recordKey; }
        }

        private readonly string recordGuid;

        public string RecordGuid
        {
            get { return recordGuid; }
        }
        public DateTime? EndOn { get; set; }
        public string SponsorReferenceCode { get; set; }
        public string CurrentStatus { get; set; }
        public IEnumerable<ReportingPeriod> ReportingPeriods { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> AccountingStrings { get; set; }
        public decimal? BudgetAmount { get; set; }
        public string ProjectType { get; set; }
        public List<string> ProjectContactPerson { get; set; }
        public string ReportingSegment { get; set; }

        /// <summary>
        /// Constructor initializes the Project.
        /// </summary>
        public ProjectCF(string recordGuid, string recordkey, string prjRefNo, DateTime? prjStartDate)
        {
            if (string.IsNullOrEmpty(recordGuid))
            {
                throw new ArgumentNullException(string.Format("Guid is required. Id:{0}", recordkey));
            }
            if (string.IsNullOrEmpty(recordkey))
            {
                throw new ArgumentNullException("ProjectCf id is required.");
            }
            if (string.IsNullOrEmpty(prjRefNo))
            {
                throw new ArgumentNullException(string.Format("Reference code is required. Guid:{0}", recordGuid));
            }
            if (!prjStartDate.HasValue)
            {
                throw new ArgumentNullException(string.Format("Start date is required. Guid:{0}", recordGuid));
            }
            this.recordGuid = recordGuid;
            this.recordKey = recordkey;
            this.referenceCode = prjRefNo;
            this.startOn = prjStartDate;
        }
    }
}
