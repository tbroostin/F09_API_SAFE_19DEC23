//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// SectionStatuses
    /// </summary>
    [Serializable]
    public class SectionStatuses : GuidCodeItem
    {
        /// <summary>
        /// Category of Section Status
        /// </summary>
        public SectionStatusIntegration Category { get; set; }

        /// <summary>
        /// Section Status Integration
        /// </summary>
        public SectionStatusIntegration? SectionStatusIntg { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public SectionStatuses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }      
    }
}