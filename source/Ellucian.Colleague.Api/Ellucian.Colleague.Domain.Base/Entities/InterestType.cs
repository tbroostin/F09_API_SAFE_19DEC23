// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Interest Type
    /// </summary>
    [Serializable]
    public class InterestType : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InterestType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The interest type category</param>
        public InterestType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}