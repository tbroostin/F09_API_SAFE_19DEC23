// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// EmployeeClassification
    /// </summary>
    [Serializable]
    public class EmploymentClassification : GuidCodeItem
    {
         /// <summary>
         /// The <see cref="EmploymentClassificationType">type</see> of employee classifcation for this entity
         /// </summary>
         private EmploymentClassificationType _employmentClassificationType;
         public EmploymentClassificationType EmploymentClassificationType { get { return _employmentClassificationType; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmploymentClassification"/> class.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="employmentClassificationType">The employment classification</param>
        /// <param name="guid"></param>
        /// <param name="code"></param>
        public EmploymentClassification(string guid, string code, string description, EmploymentClassificationType employmentClassificationType)
            : base(guid, code, description)
        {
            _employmentClassificationType = employmentClassificationType;
        }
    }
}
