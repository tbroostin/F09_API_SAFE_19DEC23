// Copyright 2014-15 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// County codes.
    /// </summary>
    [Serializable]
    public class County : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="County"/> class.
        /// </summary>
        /// <param name="guid">The guid.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public County(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}