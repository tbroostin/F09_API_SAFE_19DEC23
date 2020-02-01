﻿//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// PersonOriginCodes
    /// </summary>
    [Serializable]
    public class PersonOriginCodes : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonOriginCodes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public PersonOriginCodes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}