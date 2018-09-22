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
    /// Enumeration of possible pay statuses of the employee (with pay, without pay or partial pay).
    /// </summary>
    [Serializable]
    public enum EmployeeStatus
    {
        /// <summary>
        /// Active
        /// </summary>
        Active,

        /// <summary>
        /// Terminated
        /// </summary>
        Terminated,

        /// <summary>
        /// Leave
        /// </summary>
        Leave
    }
}
