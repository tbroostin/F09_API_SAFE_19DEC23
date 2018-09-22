// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Non Course Grade Uses
    /// </summary>
    [Serializable]
    public class NonCourseGradeUses : GuidCodeItem
    {
        /// <summary>
        /// Constructor for NonCourseGradeUses
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public NonCourseGradeUses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
