// Copyright 2014-2016 Ellucian Company L.P. and its acffiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// A valid section status code
    /// </summary>
    [Serializable]
    public class SectionStatusCode : CodeItem
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
        /// <param name="code">Actual status code</param>
        /// <param name="description">Code description</param>
        /// <param name="type">Type of code</param>
        public SectionStatusCode(string code, string description, SectionStatus? type)
            : base(code, description)
        {
            StatusType = type;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="code">Actual status code</param>
        /// <param name="description">Code description</param>
        /// <param name="type">Type of code</param>
        /// <param name="integrationType">Type of code for Integration</param>
        public SectionStatusCode(string code, string description, SectionStatus? type, SectionStatusIntegration? integrationType)
            : base(code, description)
        {
            StatusType = type;
            IntegrationStatusType = integrationType;
        }
    }
}
