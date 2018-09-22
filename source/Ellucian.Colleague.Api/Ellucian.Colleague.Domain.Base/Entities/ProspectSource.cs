// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Prospect source codes
    /// </summary>
    [Serializable]
    public class ProspectSource : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProspectSource"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ProspectSource(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}