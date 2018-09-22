//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// SapStatuses
    /// </summary>
    [Serializable]
    public class SapStatuses : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SapStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public SapStatuses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
        public string FssiCategory { get; set; }
    }
}