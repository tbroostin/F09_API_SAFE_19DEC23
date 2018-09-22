// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Gender Identity Types
    /// </summary>
    [Serializable]
    public class GenderIdentityType : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenderIdentityType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the Gender Identity type item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public GenderIdentityType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
