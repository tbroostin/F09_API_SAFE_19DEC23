// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Application Source Code doamin entity
    /// </summary>
    [Serializable]
    public class ApplicationSource : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSource"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the ApplicationSource</param>
        /// <param name="description">Description or Title of the ApplicationSource</param>
        public ApplicationSource(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}