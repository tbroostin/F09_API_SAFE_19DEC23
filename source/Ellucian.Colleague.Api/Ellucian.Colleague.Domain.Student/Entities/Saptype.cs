//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Saptype
    /// </summary>
    [Serializable]
    public class SapType : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="Saptype"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public SapType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}