// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Races
    /// </summary>
    [Serializable]
    public class PersonRelationType : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Race"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The race type</param>
        public PersonRelationType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
