// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Building type codes
    /// </summary>
    [Serializable]
    public class BuildingType : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public BuildingType(string code, string description) : base(code, description)
        {
        }
    }
}
