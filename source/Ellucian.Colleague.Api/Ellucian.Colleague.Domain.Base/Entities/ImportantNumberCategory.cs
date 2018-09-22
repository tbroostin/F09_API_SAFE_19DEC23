// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Important number category codes
    /// </summary>
    [Serializable]
    public class ImportantNumberCategory : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportantNumberCategory"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ImportantNumberCategory(string code, string description) : base(code, description)
        {
        }
    }
}
