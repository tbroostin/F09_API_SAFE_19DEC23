//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Saptype
    /// </summary>
    [Serializable]
    public class AdministrativeInstructionalMethod : GuidCodeItem
    {
        /// <summary>
        /// Points to the primary key GUID (instructional-methods)
        /// </summary>
        public string InstructionalMethodGuid { get; private set; }  
        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrativeInstructionalMethod"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="instructionalMethodGuid">GUID for the primary key</param>
        public AdministrativeInstructionalMethod(string guid, string code, string description, string instructionalMethodGuid)
            : base(guid, code, description)
        {
            InstructionalMethodGuid = instructionalMethodGuid;
        }
    }
}