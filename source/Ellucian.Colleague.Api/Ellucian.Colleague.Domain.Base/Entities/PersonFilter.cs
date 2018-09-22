// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// PersonFilter
    /// </summary>
    [Serializable]
    public class PersonFilter : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonFilter"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the PersonFilter</param>
        /// <param name="description">Description or Title of the PersonFilter</param>
        public PersonFilter(string guid, string code, string description)
            : base (guid, code, description)
        {
        }
    }
}
