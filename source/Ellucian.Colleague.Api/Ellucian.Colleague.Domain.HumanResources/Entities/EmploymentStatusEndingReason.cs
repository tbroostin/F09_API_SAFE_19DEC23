//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Employment status ending reason.
    /// </summary>
    [Serializable]
    public class EmploymentStatusEndingReason: GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmploymentStatusEndingReason"/> class.
        /// </summary>
        public EmploymentStatusEndingReason(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
