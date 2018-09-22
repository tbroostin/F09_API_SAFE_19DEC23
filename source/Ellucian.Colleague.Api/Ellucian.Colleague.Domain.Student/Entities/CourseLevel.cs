// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Course level codes
    /// </summary>
    [Serializable]
    public class CourseLevel : GuidCodeItem
    {
        /// <summary>
        /// Constructor for CourseLevel
        /// </summary>
        /// <param name="guid">Course level GUID</param>
        /// <param name="code">Course level code</param>
        /// <param name="description">Course level description</param>
        public CourseLevel(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
