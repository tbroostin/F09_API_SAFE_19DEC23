//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// EmploymentDepartment
    /// </summary>
    [Serializable]
    public class EmploymentDepartment : GuidCodeItem
    {
        /// <summary>
        /// status of the employment department code entry
        /// </summary>
        public EmploymentDepartmentStatuses? Status { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmploymentDepartment"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public EmploymentDepartment(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}