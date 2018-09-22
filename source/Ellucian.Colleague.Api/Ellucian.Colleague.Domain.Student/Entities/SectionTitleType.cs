//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// IntgSecTitleTypes
    /// </summary>
    [Serializable]
    public class SectionTitleType : GuidCodeItem
    {

        /// <summary>
        /// The title of the section title type, such as "Section short title," as opposed to the
        /// description which can be much longer and suggest usage.
        /// </summary>
        public string Title;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionTitleType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public SectionTitleType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}