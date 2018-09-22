//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// ArCategories
    /// </summary>
    [Serializable]
    public class ArCategory : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="ArCategories"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ArCategory(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}