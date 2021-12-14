// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section's Course Type
    /// </summary>
    [Serializable]
    public class SectionCourseType
    {    
        /// <summary>
        /// Course Type code
        /// </summary>
        public string Code{ get; private set; }
        /// <summary>
        /// Course Type description
        /// </summary>
        public string Description { get; private set; }

        public SectionCourseType(string code, string description)
        {
            Code = code;
            Description = description;
        }

    }
}

