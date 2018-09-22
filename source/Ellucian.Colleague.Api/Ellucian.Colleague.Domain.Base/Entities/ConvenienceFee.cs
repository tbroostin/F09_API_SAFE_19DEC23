// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Convenience fee codes
    /// </summary>
    [Serializable]
    public class ConvenienceFee : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvenienceFee"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ConvenienceFee(string code, string description)
            : base(code, description)
        {
        }
    }
}
