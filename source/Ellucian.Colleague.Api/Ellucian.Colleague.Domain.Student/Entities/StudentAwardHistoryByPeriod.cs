// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Financial award history
    /// Used within the Ethos Data Model APIs to represent a Student Financial Aid award.
    /// </summary>
    [Serializable]
    public class StudentAwardHistoryByPeriod
    {
        /// <summary>
        /// The period in which the award was bestowed.
        /// </summary>
        public string AwardPeriod { get; set; }

        /// <summary>
        /// Status of the financial aid award.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Date for the status of the financial aid award.
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Amount of award offered
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Amount of award accepted
        /// </summary>
        public decimal? XmitAmount { get; set; }

        public List<StudentAwardHistoryStatus> StatusChanges { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StudentAwardHistoryByPeriod()
        {
        }
    }
}