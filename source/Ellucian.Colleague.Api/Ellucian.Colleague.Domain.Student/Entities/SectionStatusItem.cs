// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A single grouping of the status of a section
    /// </summary>
    [Serializable]
    public class SectionStatusItem
    {
        /// <summary>
        /// The status of a section as of the specified date
        /// </summary>
        public SectionStatus Status { get; set; }

        /// <summary>
        /// The status of a section for integration as of the specified date
        /// </summary>
        public SectionStatusIntegration IntegrationStatus { get; set; }

        /// <summary>
        /// The actual status code corresponding to the status
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// The effective date of this status
        /// </summary>
        public DateTime Date { get; set; }

        public SectionStatusItem(SectionStatus status, string code, DateTime date)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (date == default(DateTime))
            {
                throw new ArgumentNullException("date");
            }

            Status = status;
            StatusCode = code;
            Date = date;
        }

        public SectionStatusItem(SectionStatus status, SectionStatusIntegration integrationStatus, string code, DateTime date)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (date == default(DateTime))
            {
                throw new ArgumentNullException("date");
            }

            Status = status;
            IntegrationStatus = integrationStatus;
            StatusCode = code;
            Date = date;
        }
    }
}
