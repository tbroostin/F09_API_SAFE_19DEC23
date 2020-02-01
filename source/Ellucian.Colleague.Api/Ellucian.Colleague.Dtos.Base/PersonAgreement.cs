// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An agreement between the institution and a person
    /// </summary>
    public class PersonAgreement
    {
        /// <summary>
        /// Unique person agreement identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Person identifier
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Agreement code
        /// </summary>
        public string AgreementCode { get; set; }

        /// <summary>
        /// Agreement period code
        /// </summary>
        public string AgreementPeriodCode { get; set; }

        /// <summary>
        /// Flag indicating whether or not the person may decline the agreement
        /// </summary>
        public bool PersonCanDeclineAgreement { get; set; }

        /// <summary>
        /// Title of the person agreement
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Date by which person must take action on the agreement
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Text of the person agreement
        /// </summary>
        public IEnumerable<string> Text { get; set; }

        /// <summary>
        /// Status of the person agreement
        /// </summary>
        public PersonAgreementStatus? Status { get; set; }

        /// <summary>
        /// Date and time at which action was taken on the agreement
        /// </summary>
        public DateTimeOffset? ActionTimestamp { get; set; }
    }
}