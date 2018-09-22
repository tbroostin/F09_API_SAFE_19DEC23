//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;


namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// CorrStatuses
    /// </summary>
    [Serializable]
    public class CorrStatus : GuidCodeItem
    {
        /// <summary>
        /// Special Processing field 1 from Valcode table
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public CorrStatus(string guid, string code, string description)
            : base(guid, code, description)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrStatuses"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="action">Speical Processing 1 from Valcode</param>
        public CorrStatus(string guid, string code, string description, string action)
            : base(guid, code, description)
        {
            Action = action;
        }
    }
}