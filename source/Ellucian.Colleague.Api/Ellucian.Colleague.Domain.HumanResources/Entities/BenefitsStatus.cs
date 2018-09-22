/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Enumeration of possible benefit statuses of the employee (with benefits or without benefits).
    /// </summary>
    [Serializable]
    public enum BenefitsStatus
    {
        /// <summary>
        /// With Benefits
        /// </summary>
        WithBenefits,

        /// <summary>
        /// Without Benefits
        /// </summary>
        WithoutBenefits
    }
}
