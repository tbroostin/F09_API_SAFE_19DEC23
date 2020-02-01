/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Enumeration of possible pay statuses of the employee (with pay, without pay or partial pay).
    /// </summary>
    [Serializable]
    public enum EmploymentDepartmentStatuses
    {
        /// <summary>
        /// Active
        /// </summary>
        Active,

        /// <summary>
        /// Inactive
        /// </summary>
        Inactive
    }
}
