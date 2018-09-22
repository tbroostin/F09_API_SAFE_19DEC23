// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Bargaining Unit
    /// </summary>
    [Serializable]
    public class BargainingUnit : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BargainingUnit"/> class.
        /// </summary>
        /// <param name="guid">A Unique Identifier for the Code</param>
        /// <param name="code">Code representing the Bargaining Unit</param>
        /// <param name="description">Description or Title of the Bargaining Unit </param>
        public BargainingUnit(string guid, string code, string description)
            : base (guid, code, description)
        {

        }
    }
}
