// Copyright 2017 Ellucian Company L.P. and its acffiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A valid section status code
    /// </summary>
    [Serializable]
    public class SectionStatusCodeGuid : GuidCodeItem
    {
        /// <summary>
        /// The type of section status this is
        /// </summary>
        public SectionStatus? StatusType { get; set; }

        /// <summary>
        /// The type of section status this is
        /// </summary>
        public SectionStatusIntegration? IntegrationStatusType { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type of code</param>
        /// <param name="code">Actual status code</param>
        /// <param name="description">Code description</param>
        public SectionStatusCodeGuid(string guid, string code, string description)
            : base(guid ,code, description)
        {

        }
    }
}
