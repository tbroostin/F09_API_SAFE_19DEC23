// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Registration marketing source
    /// </summary>
    [Serializable]
    public class RegistrationMarketingSource : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationMarketingSource"/> class.
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="description">The description</param>
        public RegistrationMarketingSource(string code, string description)
            : base(code, description)
        {
        }
    }
}