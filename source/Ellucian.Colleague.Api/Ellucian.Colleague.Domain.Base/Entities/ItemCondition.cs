// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Item Condition.
    /// </summary>
    [Serializable]
    public class ItemCondition : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCondition"/> class.
        /// </summary>
        /// <param name="guid">Unique Identifier.</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ItemCondition(string guid, string code, string description, string type = null) : base(guid, code, description)
        {
        }
    }
}