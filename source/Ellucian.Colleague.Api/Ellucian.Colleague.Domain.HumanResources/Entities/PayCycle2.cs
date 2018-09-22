//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PayCycle2
    /// </summary>
    [Serializable]
    public class PayCycle2 : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="PayCycle2"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public PayCycle2(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}