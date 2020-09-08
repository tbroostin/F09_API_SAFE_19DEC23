//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// EducationGoals
    /// </summary>
    [Serializable]
    public class EducationGoals : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EducationGoals"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public EducationGoals(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}