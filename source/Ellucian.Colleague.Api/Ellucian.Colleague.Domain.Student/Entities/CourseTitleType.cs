//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// IntgCrsTitleTypes
    /// </summary>
    [Serializable]
    public class CourseTitleType : GuidCodeItem
    {
        /// <summary>
        /// The title of the course title type, such as "Course short title," as opposed to the
        /// description which can be much longer and suggest usage.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseTitleType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CourseTitleType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}