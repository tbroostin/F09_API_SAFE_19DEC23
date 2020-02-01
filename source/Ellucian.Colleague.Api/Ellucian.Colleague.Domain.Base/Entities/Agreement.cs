// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// An agreement that can be assigned to users
    /// </summary>
    [Serializable]
    public class Agreement : CodeItem
    {
        /// <summary>
        /// Flag indicating whether or not users may decline the agreement
        /// </summary>
        public bool UsersCanDeclineAgreement { get; private set; }

        /// <summary>
        /// Text of the agreement
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Default agreement period to which the agreement applies
        /// </summary>
        public string DefaultAgreementPeriod { get; set; }

        /// <summary>
        /// Default date on which users must submit consent for the agreement
        /// </summary>
        public DateTime? DefaultDueDate { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Agreement"/> object.
        /// </summary>
        /// <param name="code">Agreement code</param>
        /// <param name="description">Agreement description</param>
        /// <param name="usersCanDeclineAgreement">Flag indicating whether or not users may decline the agreement</param>
        /// <param name="text">Text of the agreement</param>
        public Agreement(string code, string description, bool usersCanDeclineAgreement, string text) : base(code, description)
        {
            UsersCanDeclineAgreement = usersCanDeclineAgreement;
            Text = text;
        }
    }
}
