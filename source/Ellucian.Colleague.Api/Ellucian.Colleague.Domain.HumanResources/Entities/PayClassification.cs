/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PayScale
    /// </summary>
    [Serializable]
    public class PayClassification : GuidCodeItem
    {
        /// <summary>
        /// The compensation type associated with the pay classification (e.g. salary or wages).
        /// </summary>
        public string CompensationType { get; set; }

        /// <summary>
        /// The type of pay classification (e.g. matrix or range).
        /// </summary>
        public string ClassificationType { get; set; }

        /// <summary>
        /// The status of the pay classification (e.g. active or inactive).
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Create a PayClassification
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="code">A code that may be used to identify the pay classification.</param>
        /// <param name="description">The full name of the pay classification.</param>
        public PayClassification(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}