// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// An agreement between the institution and a person
    /// </summary>
    [Serializable]
    public class PersonAgreement
    {
        /// <summary>
        /// Unique person agreement identifier
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Person identifier
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// Agreement code
        /// </summary>
        public string AgreementCode { get; private set; }

        /// <summary>
        /// Agreement period code
        /// </summary>
        public string AgreementPeriodCode { get; private set; }

        /// <summary>
        /// Flag indicating whether or not the person may decline the agreement
        /// </summary>
        public bool PersonCanDeclineAgreement { get; private set; }

        /// <summary>
        /// Title of the person agreement
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Date by which person must take action on the agreement
        /// </summary>
        public DateTime DueDate { get; private set; }

        /// <summary>
        /// Text of the person agreement
        public IEnumerable<string> Text { get; private set; }

        /// <summary>
        /// Status of the person agreement
        /// </summary>
        public PersonAgreementStatus? Status { get; private set; }

        /// <summary>
        /// Date and time at which action was taken on the agreement
        /// </summary>
        public DateTimeOffset? ActionTimestamp { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="PersonAgreement"/> object.
        /// </summary>
        /// <param name="id">Unique person agreement identifier</param>
        /// <param name="personId">Person identifier</param>
        /// <param name="agreementCode">Agreement code</param>
        /// <param name="agreementPeriodCode">Agreement period code</param>
        /// <param name="personCanDeclineAgreement">Flag indicating whether or not the person may decline the agreement</param>
        /// <param name="title">Title of the person agreement</param>
        /// <param name="dueDate">Date on which person must take action on the agreement</param>
        /// <param name="text">Text of the person agreement</param>
        /// <param name="status">Status of the person agreement</param>
        /// <param name="actionTimestamp">Date and time at which action was taken on the agreement</param>
        public PersonAgreement(string id, string personId, string agreementCode, string agreementPeriodCode, bool personCanDeclineAgreement, string title, DateTime dueDate, IEnumerable<string> text, PersonAgreementStatus? status, DateTimeOffset? actionTimestamp)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A person agreement must have a unique identifier.");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person agreement must have a person identifier.");
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException("title", "A person agreement must have a title.");
            }
            if (string.IsNullOrEmpty(agreementCode))
            {
                throw new ArgumentNullException("agreementCode", "A person agreement must have an agreement code.");
            }
            if (string.IsNullOrEmpty(agreementPeriodCode))
            {
                throw new ArgumentNullException("agreementPeriodCode", "A person agreement must have an agreement period code.");
            }
            if (!personCanDeclineAgreement && status == PersonAgreementStatus.Declined)
            {
                throw new ArgumentException("status", "Declined status cannot be given to a person agreement that does not allow the person to decline.");
            }
            Id = id;
            PersonId = personId;
            AgreementCode = agreementCode;
            AgreementPeriodCode = agreementPeriodCode;
            PersonCanDeclineAgreement = personCanDeclineAgreement;
            Title = title;
            DueDate = dueDate;
            Text = text != null ? text.ToList() : new List<string>();
            Status = status;
            ActionTimestamp = actionTimestamp;
        }
    }
}
