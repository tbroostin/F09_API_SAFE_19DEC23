// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Non Course Categories codes
    /// </summary>
    [Serializable]
    public class NonCourseCategories : GuidCodeItem
    {

        public string SpecialProcessingCode { get; set; }
        /// <summary>
        /// Constructor for NonCourseCategories
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public NonCourseCategories(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
