//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// CourseStatuses
    /// </summary>
    [Serializable]
    public class CourseStatuses : GuidCodeItem
    {
        /// <summary>
        /// The course status
        /// </summary>
        public CourseStatus Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CourseStatuses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}