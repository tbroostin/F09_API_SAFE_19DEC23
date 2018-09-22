//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// EmploymentTerminationReason
    /// </summary>
    [Serializable]
    public class EmploymentTerminationReason : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmploymentTerminationReason"/> class.
        /// </summary>
        /// <param name="employmentClassificationType">The employment classification</param>
         public EmploymentTerminationReason(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
