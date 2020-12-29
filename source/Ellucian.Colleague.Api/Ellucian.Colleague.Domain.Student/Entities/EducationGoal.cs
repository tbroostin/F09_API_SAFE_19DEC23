// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Education goal
    /// </summary>
    [Serializable]
    public class EducationGoal : CodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EducationGoal"/> class.
        /// </summary>
        /// <param name="code">The code</param>
        /// <param name="description">The description</param>
        public EducationGoal(string code, string description)
            : base(code, description)
        {
        }
    }
}