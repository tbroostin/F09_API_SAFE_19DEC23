// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Grade Change Reason
    /// </summary>
    [Serializable]
    public class GradeChangeReason: GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GradeChangeReason"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier for the GradeChangeReason item</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public GradeChangeReason(string guid, string code, string description)
            : base(guid, code, description)
        {

        }
    }
}
