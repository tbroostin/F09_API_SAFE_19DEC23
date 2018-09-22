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
    /// Enumeration of possible types of employment (Full-time, Part-time or Contractual).
    /// </summary>
    [Serializable]
    public enum ContractType
    {
        /// <summary>
        /// Full-time
        /// </summary>
        FullTime,

        /// <summary>
        /// Part-time
        /// </summary>
        PartTime,

        /// <summary>
        /// Contractual
        /// </summary>
        Contractual
    }
}
