// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Vocation
    /// </summary>
    [Serializable]
    public class Vocation : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisaType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public Vocation(string guid, string code, string description)
            : base(guid, code, description)
        {
            // no additional work to do
        }
    }
}