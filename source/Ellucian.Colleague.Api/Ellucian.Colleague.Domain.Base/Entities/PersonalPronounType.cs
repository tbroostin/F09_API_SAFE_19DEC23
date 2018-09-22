// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Personal Pronoun Types
    /// </summary>
    [Serializable]
    public class PersonalPronounType: GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalPronounType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the Personal Pronoun type item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public PersonalPronounType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
