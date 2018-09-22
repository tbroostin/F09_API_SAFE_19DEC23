// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A demographic field potentially collected when adding persons to Colleague via Proxy interfaces
    /// </summary>
    public class DemographicField
    {
        /// <summary>
        /// Unique identifier for the demographic field
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description of the demographic field
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Field requirement type
        /// </summary>
        public DemographicFieldRequirement Requirement { get; set; }

        /// <summary>
        /// Field type
        /// </summary>
        public DemographicFieldType Type { get; set; }
    }
}
