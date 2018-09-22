using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Institution Type Enumeration
    /// </summary>
    public enum InstType
    {
        /// <summary>
        /// College
        /// </summary>
        College,
        /// <summary>
        /// High School
        /// </summary>
        HighSchool,
        /// <summary>
        /// Neither High School or College (should never happen)
        /// </summary>
        Unknown
    }
}
