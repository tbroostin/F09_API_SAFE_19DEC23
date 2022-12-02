//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// IntgSecDescTypes
    /// </summary>
    [Serializable]
    public class SectionDescriptionType : GuidCodeItem
    {

        /// <summary>
        /// The description of the section description type, such as "Section short title," as opposed to the
        /// description which can be much longer and suggest usage.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionDescriptionType"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public SectionDescriptionType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}