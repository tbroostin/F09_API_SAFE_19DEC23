// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Academic department does
    /// </summary>
    [Serializable]
    public class AcademicDepartment : Department
    {
        /// <summary>
        /// Default academic level code for the department
        /// </summary>
        public string AcademicLevelCode { get; set; }

        /// <summary>
        /// Default grade scheme code for the department
        /// </summary>
        public string GradeSchemeCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AcademicDepartment"/> class.
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public AcademicDepartment(string guid, string code, string description, bool isActive)
            : base(guid, code, description, isActive)
        {
        }
    }
}