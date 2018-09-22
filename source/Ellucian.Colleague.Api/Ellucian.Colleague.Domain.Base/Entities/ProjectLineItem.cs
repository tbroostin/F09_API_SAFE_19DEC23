// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// ProjectLineItem
    /// </summary>
    [Serializable]
    public class ProjectLineItem : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectLineItem"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the ProjectLineItem item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ProjectLineItem(string guid, string code, string description)
            : base(guid, code, description)
        {

        }
    }
}
