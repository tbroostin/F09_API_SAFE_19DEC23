// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of possible types of an email address
    /// </summary>
    [Serializable]
    public enum PersonEmailType
    {
        /// <summary>
        /// A person's personal email address.
        /// </summary>
        Personal,
        /// <summary>
        /// A person's business email address.
        /// </summary>
        Business,
        /// <summary>
        /// A person's school email address.
        /// </summary>
        School,
        /// <summary>
        /// Uncategorized email address.
        /// </summary>
        Other
    }
}
