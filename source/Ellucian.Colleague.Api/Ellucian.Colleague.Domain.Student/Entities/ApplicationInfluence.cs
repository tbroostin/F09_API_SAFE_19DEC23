// Copyright 2013-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ApplicationInfluence : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInfluence"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ApplicationInfluence(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}