//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// HumanResourcesInstitutionEmployer
    /// </summary>
    [Serializable]
    public class HumanResourcesInstitutionEmployer : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="HumanResourcesInstitutionEmployer"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public HumanResourcesInstitutionEmployer(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}